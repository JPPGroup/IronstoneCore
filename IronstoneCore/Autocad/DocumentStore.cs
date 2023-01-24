using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Jpp.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Jpp.Ironstone.Core.Autocad
{
    //TODO: Rework to use reflection, no need to override save and load?
    /// <summary>
    /// Class for storing of document level data
    /// </summary>
    public class DocumentStore : DisposableManagedObject
    {
        [XmlIgnore] public bool ShouldUnlockUnfreeze { get; }
        [XmlIgnore] public bool ShouldSwitchOn { get;  }

        #region Constructor and Fields

        public event EventHandler<DocumentNameChangedEventArgs> DocumentNameChanged;
        
        protected Document AcDoc { get; set; }
        protected Database AcCurDb { get; set; }

        public List<IDrawingObjectManager> Managers { get; private set; }
        private readonly Type[] _managerTypes;
        private readonly ILogger<CoreExtensionApplication> _log;
        private readonly LayerManager _layerManager;
        private readonly Document _host;
        private IConfiguration _settings;

        public LayerManager LayerManager
        {
            get { return _layerManager; }
        }

        /// <summary>
        /// Create a new document store
        /// </summary>
        public DocumentStore(Document doc, Type[] managerTypes, ILogger<CoreExtensionApplication> log, LayerManager lm, IConfiguration settings)
        {
            if(doc is null)
                throw new ArgumentNullException(nameof(doc));

            if(settings is null)
                throw new ArgumentNullException(nameof(settings));

            AcDoc = doc;
            AcCurDb = doc.Database;

            _managerTypes = managerTypes;
            _log = log;
            _layerManager = lm;
            _host = doc;
            _settings = settings;

            ShouldUnlockUnfreeze = bool.TryParse(settings["layers-unlock-unfreeze"], out var unlockUnfreeze) && unlockUnfreeze;
            ShouldSwitchOn = bool.TryParse(settings["layers-switch-on"], out var switchOn) && switchOn;

            Managers = new List<IDrawingObjectManager>();

            _host.Database.BeginSave += (o, args) => BeginSave(args.FileName);
            _host.CommandEnded += (o, args) => CommandEnded(args.GlobalCommandName);
            _host.CommandCancelled += (o, args) => CommandEnded(args.GlobalCommandName);

            _log.LogDebug("{storetype} created", this.GetType().ToString());
            PopulateLayers();
        }

        private void PopulateLayers()
        {
            var layers = GetLayers();
            if (layers.Count == 0) return;

            using (AcDoc.LockDocument())
            {
                using (var trans = AcCurDb.TransactionManager.StartTransaction())
                {
                    foreach (object layerObj in layers)
                    {
                        if (layerObj is LayerAttribute layer)
                        {
                            _layerManager.CreateLayer(AcCurDb, layer.Name);
                        }
                    }

                    trans.Commit();
                }
            }
        }

        private List<object> GetLayers()
        {
            var type = GetType();
            var layers = type.GetCustomAttributes(typeof(LayerAttribute), true).ToList();

            foreach (var objManager in Managers)
            {
                layers.AddRange(objManager.GetRequiredLayers());
            }

            return layers;
        }

        private List<LayerState> ActivateLayers()
        {
            var layersToRevert = new List<LayerState>();
            var layers = GetLayers();

            if (layers.Count == 0) return layersToRevert;

            //Need a transaction, not sure where else it could come from.
            using (var acTrans = AcCurDb.TransactionManager.StartTransaction()) 
            {
                foreach (var layerObj in layers)
                {
                    if (layerObj is LayerAttribute layerAttribute)
                    {
                        try
                        {
                            string layerName = _layerManager.GetLayerName(layerAttribute.Name);
                            var layer = AcCurDb.GetLayer(layerName);
                            if (layer == null)
                            {
                                // TODO: Redo this to use pull the correct name from the layer manager
                                _log.LogError("Layer {name} not found when attempting to save state.", layerName);
                                continue;
                            }

                            var status = new LayerState(layer);

                            if (status.IsInvalid)
                            {
                                layersToRevert.Add(status);

                                if (ShouldUnlockUnfreeze || ShouldSwitchOn)
                                {
                                    layer.UpgradeOpen();

                                    if (ShouldUnlockUnfreeze)
                                    {
                                        layer.IsLocked = false;
                                        layer.IsFrozen = false;
                                    }

                                    if (ShouldSwitchOn)
                                    {
                                        layer.IsOff = false;
                                    }
                                }
                            }
                        }
                        catch (ArgumentNullException)
                        {
                            _log.LogError("Layer key {name} not found when attempting to save state.", layerAttribute.Name);
                        }
                    }
                }

                acTrans.Commit();
            }

            return layersToRevert;
        }

        private void RevertLayers(IReadOnlyCollection<LayerState> layers)
        {
            if (layers.Count == 0) return;

            //Need a transaction, not sure where else it could come from.
            using (var acTrans = AcCurDb.TransactionManager.StartTransaction()) 
            {
                foreach (var revert in layers)
                {
                    var layer = AcCurDb.GetLayer(revert.Name);

                    layer.UpgradeOpen();
                    layer.IsLocked = revert.IsLocked;
                    layer.IsFrozen = revert.IsFrozen;
                    layer.IsOff = revert.IsOff;
                }

                acTrans.Commit();
            }
        }

        #endregion

        private void BeginSave(string fileName)
        {
            SaveWrapper();

            //check if file extension is valid, used to ignore auto-save file
            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(extension) || !Constants.ValidDocumentExtensions.Contains(extension.ToLower()))
            {
                _log.LogDebug($"Assumed AutoSave. Filename: {fileName}");
                return;
            }

            if (_host.Name == fileName) return;
            var hostDoc = _host.Name;

            // ReSharper disable once ConvertToLocalFunction
            DatabaseIOEventHandler handler = null;
            handler = (sender, args) =>
            {
                _host.Database.SaveComplete -= handler;
                DocumentNameChangeOnSave(hostDoc, fileName);
            };

            _host.Database.SaveComplete += handler;
        }

        private void DocumentNameChangeOnSave(string oldName, string newName)
        {
            DocumentNameChangedEventArgs args = new DocumentNameChangedEventArgs { OldName = oldName, NewName = newName };
            OnDocumentNameChanged(args);
        }

        protected virtual void OnDocumentNameChanged(DocumentNameChangedEventArgs e)
        {
            EventHandler<DocumentNameChangedEventArgs> handler = DocumentNameChanged;
            handler?.Invoke(this, e);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "No errors should propogate to Autocad to prevent crash")]
        private void CommandEnded(string globalCommandName)
        {
            _log.LogTrace("CommandEnded after {commandname} started for {storetype}", globalCommandName, this.GetType().ToString());
            try
            {
                var layersToRevert = ActivateLayers();

                if (!ShouldUnlockUnfreeze || !ShouldSwitchOn)
                {
                    var inactiveLayers = new List<string>();

                    if (!ShouldUnlockUnfreeze)
                    {
                        var frozenLocked = layersToRevert.Where(l => l.IsFrozen || l.IsLocked).Select(l => l.Name).ToList();
                        inactiveLayers = inactiveLayers.Union(frozenLocked).ToList();
                    }

                    if (!ShouldSwitchOn)
                    {
                        var off = layersToRevert.Where(l => l.IsOff).Select(l => l.Name).ToList();
                        inactiveLayers = inactiveLayers.Union(off).ToList();
                    }

                    if (inactiveLayers.Count > 0)
                    {
                        var layersList = string.Join(", ", inactiveLayers.ToArray());
                        _log.LogWarning($"Following layers need to be active; \n{layersList}");
                        return;
                    }
                }

                if (globalCommandName.ToLower().Contains("regen"))
                {
                    RegenerateManagers();
                }
                else
                {
                    UpdateManagers();
                }

                RevertLayers(layersToRevert);
                _log.LogTrace("CommandEnded complete for {storetype}", this.GetType().ToString());
            }
            catch (Exception e)
            {
                _log.LogError(e, "Unexpected error in command ended event for {storetype}", this.GetType().ToString());
            }
            
        }

        #region Save and Load Methods
        /// <summary>
        /// Save all fields in class
        /// </summary>
        protected virtual void Save()
        {
            //Doesnt have nay default fields to save
        }


        /// <summary>
        /// Load all fields in class
        /// </summary>
        protected virtual void Load()
        {
            //Doesnt have any default fields to load
        }


        /// <summary>
        /// Wrapper around the save method to ensure a transaction is active when called
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "All exceptions need to be caught to prevent autocad crashes")]
        internal void SaveWrapper()
        {
            _log.LogDebug("SaveWrapper started for {storetype}", this.GetType().ToString());
            try
            {
                using (DocumentLock dl = AcDoc?.LockDocument())
                {
                    using (Transaction tr = AcCurDb.TransactionManager.StartTransaction())
                    {
                        var mgrObjList = new List<object>();
                        Managers.ForEach(mgr => mgrObjList.Add(mgr));
                        if (mgrObjList.Count > 0)
                        {
                            _log.LogDebug("{managercount} managers were added to save list", mgrObjList.Count);
                        }
                        foreach (var mgrObj in mgrObjList)
                        {
                            _log.LogTrace("{managername} on save list", mgrObj.GetType().ToString() );
                        }

                        SaveBinary("Managers", mgrObjList, _managerTypes);
                        Save();
                        tr.Commit();
                    }
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "Unkown error saving for {storetype}", this.GetType().ToString());
            }
        }

        /// <summary>
        /// Wrapper around the load method to ensure a transaction is active when called
        /// </summary> 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "All exceptions need to be caught to prevent autocad crashes")]
        internal void LoadWrapper()
        {
            _log.LogDebug("LoadWrapper started for {storetype}", this.GetType().ToString());
            try
            {
                using (DocumentLock dl = AcDoc?.LockDocument())
                {
                    using (Transaction tr = AcCurDb.TransactionManager.StartTransaction())
                    {
                        Managers.Clear();

                        var mgrObjList = LoadBinary<List<object>>("Managers", _managerTypes);
                        _log.LogDebug("{managercount} managers were found and loaded", mgrObjList.Count);
                        foreach (var mgrObj in mgrObjList)
                        {
                            _log.LogTrace("{managername} loaded", mgrObj.GetType().ToString());
                        }

                        foreach (IDrawingObjectManager drawingObjectManager in mgrObjList)
                        {
                            drawingObjectManager.SetDependencies(AcDoc, _log, _settings);
                            drawingObjectManager.ActivateObjects();
                            Managers.Add(drawingObjectManager);
                        }
                        Load();
                        PopulateLayers();
                        tr.Commit();
                    }
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "Unkown error loading for {storetype}", this.GetType().ToString());
            }
        }
        #endregion

        #region Binary Methods
        protected void SaveBinary(string key, object binaryObject, Type[] additionalTypes = null)
        {
            //Database acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
            Transaction tr = AcCurDb.TransactionManager.TopTransaction;

            // Find the NOD in the database
            DBDictionary nod = (DBDictionary)tr.GetObject(AcCurDb.NamedObjectsDictionaryId, OpenMode.ForWrite);

            // We use Xrecord class to store data in Dictionaries
            Xrecord plotXRecord = new Xrecord();

            XmlSerializer xml;

            if (additionalTypes == null)
            {
                xml = new XmlSerializer(binaryObject.GetType());
            }
            else
            {
                xml = new XmlSerializer(binaryObject.GetType(), additionalTypes);
            }

            using (MemoryStream ms = new MemoryStream())
            {
                xml.Serialize(ms, binaryObject);
                string s = Encoding.ASCII.GetString(ms.ToArray());

                byte[] data = new byte[512];
                int moreData = 1;
                ResultBuffer rb = new ResultBuffer();
                ms.Position = 0;
                while (moreData > 0)
                {
                    data = new byte[512];
                    moreData = ms.Read(data, 0, data.Length);
                    string dataString = Encoding.ASCII.GetString(data);
                    TypedValue tv = new TypedValue((int) DxfCode.Text, dataString);
                    rb.Add(tv);
                }

                plotXRecord.Data = rb;
            }

            // Create the entry in the Named Object Dictionary
            string id = this.GetType().FullName + key;
            nod.SetAt(id, plotXRecord);
            tr.AddNewlyCreatedDBObject(plotXRecord, true);
        }

        protected T LoadBinary<T>(string key, Type[] additionalTypes = null) where T : new()
        {
            Transaction tr = AcCurDb.TransactionManager.TopTransaction;

            // Find the NOD in the database
            DBDictionary nod = (DBDictionary)tr.GetObject(AcCurDb.NamedObjectsDictionaryId, OpenMode.ForWrite);

            string id = this.GetType().FullName + key;

            if (nod.Contains(id))
            {
                ObjectId plotId = nod.GetAt(id);
                Xrecord plotXRecord = (Xrecord)tr.GetObject(plotId, OpenMode.ForRead);
                using (MemoryStream ms = new MemoryStream())
                {
                    foreach (TypedValue value in plotXRecord.Data)
                    {
                        byte[] data = new byte[512];

                        string message = (string) value.Value;
                        data = Encoding.ASCII.GetBytes(message);
                        ms.Write(data, 0, data.Length);
                    }

                    ms.Position = 0;

                    XmlSerializer xml;

                    if (additionalTypes == null)
                    {
                        xml = new XmlSerializer(typeof(T));
                    }
                    else
                    {
                        xml = new XmlSerializer(typeof(T), additionalTypes);
                    }

                    try
                    {
                        string s = Encoding.ASCII.GetString(ms.ToArray());
                        return (T) xml.Deserialize(ms);
                    }
                    catch (Exception e)
                    {
                        _log.LogError(e, "Unkown error loading binary");
                        return new T();
                    }
                }
            }

            //TODO: check changing from default has not broken this
            return new T();
        }

        protected override void DisposeManagedResources()
        {
            SaveWrapper();
            base.DisposeManagedResources();
        }

        public void UpdateManagers()
        {
            AcCurDb.DisableUndoRecording(true);

            foreach (IDrawingObjectManager drawingObjectManager in Managers)
            {
                drawingObjectManager.UpdateDirty();
            }

            AcCurDb.DisableUndoRecording(false);
        }

        public void RegenerateManagers()
        {
            AcCurDb.DisableUndoRecording(true);

            foreach (IDrawingObjectManager drawingObjectManager in Managers)
            {
                drawingObjectManager.UpdateAll();
            }

            AcCurDb.DisableUndoRecording(false);
        }

        public T GetManager<T>() where T : class, IDrawingObjectManager
        {
            T foundManager = null;
            foreach (var manager in Managers)
            {
                if (manager is T objectManager) foundManager = objectManager;
            }

            if (foundManager != null) return foundManager;

            foundManager = (T)Activator.CreateInstance(typeof(T), AcDoc, _log, _settings);
            Managers.Add(foundManager);
            PopulateLayers();

            return foundManager;

        }
        #endregion
    }

    public class DocumentNameChangedEventArgs : EventArgs
    {
        public string OldName { get; set; }
        public string NewName { get; set; }
    }
}
