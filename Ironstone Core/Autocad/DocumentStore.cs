using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Common;

namespace Jpp.Ironstone.Core.Autocad
{
    //TODO: Rework to use reflection, no need to override save and load?
    /// <summary>
    /// Class for storing of document level data
    /// </summary>
    public class DocumentStore : IDisposable
    {
        #region Constructor and Fields

        protected Document acDoc;
        protected Database acCurDb;

        protected List<AbstractDrawingObjectManager> Managers;
        private Type[] _managerTypes;

        /// <summary>
        /// Create a new document store
        /// </summary>
        public DocumentStore(Document doc, Type[] ManagerTypes)
        {
            acDoc = doc;
            acCurDb = doc.Database;
            _managerTypes = ManagerTypes;
            Managers = new List<AbstractDrawingObjectManager>();
        }
        #endregion

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
        internal void SaveWrapper()
        {
            try
            {
                using (DocumentLock dl = acDoc?.LockDocument())
                {
                    using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                    {
                        SaveBinary("Managers", Managers, _managerTypes);
                        Save();
                        tr.Commit();
                    }
                }
            }
            catch (Exception e)
            {
                //TODO: Add log
                //Application.ShowAlertDialog("Error saving - " + e.Message);
            }
        }

        /// <summary>
        /// Wrapper around the load method to ensure a transaction is active when called
        /// </summary>
        internal void LoadWrapper()
        {
            try
            {
                using (DocumentLock dl = acDoc?.LockDocument())
                {
                    using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                    {
                        Managers = LoadBinary<List<AbstractDrawingObjectManager>>("Managers", _managerTypes);
                        foreach (AbstractDrawingObjectManager abstractDrawingObjectManager in Managers)
                        {
                            abstractDrawingObjectManager.HostDocument = acDoc;
                            abstractDrawingObjectManager.ActivateObjects();
                        }
                        Load();
                        tr.Commit();
                    }
                }
            }
            catch (Exception e)
            {
                //TODO: Add log
                //Application.ShowAlertDialog("Error saving - " + e.Message);
            }
        }
        #endregion

        #region Binary Methods
        protected void SaveBinary(string key, object binaryObject, Type[] additionalTypes = null)
        {
            //Database acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
            Transaction tr = acCurDb.TransactionManager.TopTransaction;

            // Find the NOD in the database
            DBDictionary nod = (DBDictionary)tr.GetObject(acCurDb.NamedObjectsDictionaryId, OpenMode.ForWrite);

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

            MemoryStream ms = new MemoryStream();
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
                TypedValue tv = new TypedValue((int)DxfCode.Text, dataString);
                rb.Add(tv);
            }

            plotXRecord.Data = rb;

            // Create the entry in the Named Object Dictionary
            string id = this.GetType().FullName + key;
            nod.SetAt(id, plotXRecord);
            tr.AddNewlyCreatedDBObject(plotXRecord, true);
        }

        protected T LoadBinary<T>(string Key, Type[] additionalTypes = null) where T : new()
        {
            //Database acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
            Transaction tr = acCurDb.TransactionManager.TopTransaction;

            // Find the NOD in the database
            DBDictionary nod = (DBDictionary)tr.GetObject(acCurDb.NamedObjectsDictionaryId, OpenMode.ForWrite);

            string id = this.GetType().FullName + Key;

            if (nod.Contains(id))
            {
                ObjectId plotId = nod.GetAt(id);
                Xrecord plotXRecord = (Xrecord)tr.GetObject(plotId, OpenMode.ForRead);
                MemoryStream ms = new MemoryStream();
                foreach (TypedValue value in plotXRecord.Data)
                {
                    byte[] data = new byte[512];

                    string message = (string)value.Value;
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
                    return (T)xml.Deserialize(ms);
                }
                catch (Exception e)
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                //TODO: check changing from default has not broken this
                return new T();
            }
        }

        public void Dispose()
        {
            SaveWrapper();
        }

        public void UpdateManagers()
        {
            foreach (IDrawingObjectManager drawingObjectManager in Managers)
            {
                drawingObjectManager.UpdateDirty();
            }
        }

        public void ReenerateManagers()
        {
            foreach (IDrawingObjectManager drawingObjectManager in Managers)
            {
                drawingObjectManager.UpdateAll();
            }
        }

        public T GetManager<T>() where T : AbstractDrawingObjectManager
        {
            /*if (Managers.ContainsKey(typeof(T)))
            {
                return (T) Managers[typeof(T)];
            }
            else
            {
                T dom = (T)Activator.CreateInstance(typeof(T), this.acDoc);
                Managers.Add(typeof(T), dom);
                return dom;
            }*/
            T foundmanager = null;
            foreach (var manager in Managers)
            {
                if (manager is T)
                {
                    foundmanager = manager as T;
                }
            }

            if (foundmanager == null)
            {
                foundmanager = (T)Activator.CreateInstance(typeof(T), this.acDoc);
                Managers.Add(foundmanager);
            }

            return foundmanager;

        }
        #endregion

        /*public static void LoadStores(Document doc)
        {
            //Get all document stores and load
            List<Type> storesList = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    storesList.AddRange(assembly.GetTypes().Where(t => typeof(DocumentStore).IsAssignableFrom(t)));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            foreach (Type type in storesList)
            {
                //Load docstore to generate managers
                doc.GetDocumentStore(type);
            }
        }*/
    }
}
