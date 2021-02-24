﻿using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Unity;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Jpp.Ironstone.Core.Autocad
{
    public abstract class DrawingObject
    {
        protected DBObject _activeObject;
        protected Database _database { get; private set; }
        
        [Obsolete]
        public string DatabaseName
        {
            get { return _database.Filename; }
            set
            {
                DocumentCollection docs = Application.DocumentManager;

                foreach (Document doc in docs)
                {
                    if (doc.Database.Filename.Equals(value))
                    {
                        _database = doc.Database;
                        return;
                    }
                }

                //TODO: Review this and try to deal with database changing name
                _database = Application.DocumentManager.MdiActiveDocument.Database;
            }
        }

        protected Document _document { get; private set; }

        public Document Document
        {
            get { return _document; }
        }

        public string DocumentName
        {
            get { return _document.Name; }
            set
            {
                DocumentCollection docs = Application.DocumentManager;

                foreach (Document doc in docs)
                {
                    if (doc.Name.Equals(value))
                    {
                        _document = doc;
                        return;
                    }
                }

                //TODO: Review this and try to deal with database changing name
                _document = Application.DocumentManager.MdiActiveDocument;
            }
        }

        //TODO: review setter
        public long BaseObjectPtr { get; set; }
        [XmlIgnore] public bool Active { get; private set; }
        [XmlIgnore]
        public ObjectId BaseObject
        {
            get
            {
                if (BaseObjectPtr == 0)
                {
                    GenerateBase();
                }

                //return _database.GetObjectId(false, new Handle(BaseObjectPtr), 0);
                if(_document != null)
                    return _document.Database.GetObjectId(false, new Handle(BaseObjectPtr), 0);

                if (_database != null)
                    return _database.GetObjectId(false, new Handle(BaseObjectPtr), 0);

                CoreExtensionApplication._current.Container.Resolve<ILogger>().Entry("Drawing object does not have database or document set, reverting to active document fopr ObjectID creation.", Severity.Error);
                return Application.DocumentManager.MdiActiveDocument.Database.GetObjectId(false, new Handle(BaseObjectPtr), 0);
            }
            set
            {
                if (BaseObjectPtr == value.Handle.Value) return;

                long oldHandle = BaseObjectPtr;
                BaseObjectPtr = value.Handle.Value;
                if (!VerifyBaseObject())
                {
                    BaseObjectPtr = oldHandle;
                    throw new ArgumentException($"Invalid base type");
                }
                
                //Remove event handles from previous object, change pointer and activate new object.
                UnhookActiveObject();
                BaseObjectPtr = value.Handle.Value;
                CreateActiveObject();
            }
        }

        [XmlIgnore]
        public int ColorIndex {
            get
            {
                Transaction trans = _database.TransactionManager.TopTransaction;
                Entity ent = (Entity)trans.GetObject(this.BaseObject, OpenMode.ForRead);
                return ent.ColorIndex;
            }
            set
            {
                Transaction trans = _database.TransactionManager.TopTransaction;
                Entity ent = (Entity)trans.GetObject(this.BaseObject, OpenMode.ForWrite);
                ent.ColorIndex = value;
            }
        }

        protected virtual bool VerifyBaseObject()
        {
            return true;
        }

        private void UnhookActiveObject()
        {
            if (!Active) return;

            Transaction acTrans = _database.TransactionManager.TopTransaction;
            _activeObject = acTrans.GetObject(BaseObject, OpenMode.ForWrite);
            _activeObject.Erased -= ActiveObject_Erased;
            _activeObject.Modified -= ActiveObject_Modified;

            Active = false;
        }

        public bool CreateActiveObject()
        {
            try
            {
                //Transaction acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                Transaction acTrans = null;
                if(_document != null)
                    acTrans = _document.TransactionManager.TopTransaction;

                if (_database != null)
                    acTrans = _database.TransactionManager.TopTransaction;

                if (acTrans == null)
                {
                    CoreExtensionApplication._current.Container.Resolve<ILogger>().Entry("Drawing object does not have database or document set, reverting to active document.", Severity.Error);
                    acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                }

                _activeObject = acTrans.GetObject(BaseObject, OpenMode.ForWrite);
                _activeObject.Erased += ActiveObject_Erased;
                _activeObject.Modified += ActiveObject_Modified;

                Active = true;

                return true;
            }
            catch (Autodesk.AutoCAD.Runtime.Exception acEx)
            {
                if (acEx.ErrorStatus == ErrorStatus.UnknownHandle) return false;

                throw;
            }
        }

        private void ActiveObject_Modified(object sender, EventArgs e)
        {
            foreach (DrawingObject subObject in SubObjects.Values)
            {
                subObject.ParentUpdated(this);
            }
            ObjectModified(sender, e);
            //TODO: Add tracking
            Verified = false;
            DirtyModified = true;
        }

        protected virtual void GenerateBase()
        {
            throw new NullReferenceException("No base object has been linked");
        }

        protected abstract void ObjectModified(object sender, EventArgs e);

        private void ActiveObject_Erased(object sender, ObjectErasedEventArgs e)
        {
            Erased = e.Erased;
            ObjectErased(sender, e);
            
            //TODO: Review once dirty system implemented
            DirtyRemoved = e.Erased;
            DirtyAdded = !e.Erased;
        }

        protected abstract void ObjectErased(object sender, ObjectErasedEventArgs e);

        public bool Erased { get; set; }

        private Dictionary<string, string> _XData;

        [XmlIgnore]
        // TODO: This needs lots of tests added
        public string this[string key]
        {
            get
            {
                if (_XData == null)
                {
                    LoadXData();
                }

                if (!_XData.ContainsKey(key))
                    throw new KeyNotFoundException();

                return _XData[key];
            }
            set
            {
                Transaction tr = BaseObject.Database.TransactionManager.TopTransaction;
                DBObject obj = tr.GetObject(BaseObject, OpenMode.ForWrite);

                ResultBuffer rb = obj.XData;
                ResultBuffer newBuffer = new ResultBuffer();
                newBuffer.Add(new TypedValue(1001, "JPP"));

                if (rb != null && _XData.ContainsKey(key))
                {
                    for(int i = 0; i < rb.AsArray().Length; i++)//foreach (TypedValue tv in rb)
                    {
                        TypedValue tv = rb.AsArray()[i];
                        string data = tv.Value as string;
                        string[] keyvalue = data.Split(':');
                        if (keyvalue[0] == key)
                        {
                            TypedValue newTypedValue = new TypedValue(1000 ,$"{keyvalue[0]}:{value}");
                            newBuffer.Add(newTypedValue);
                        }
                        else
                        {
                            newBuffer.Add(tv);
                        }
                    }
                }
                else
                {
                    TypedValue newTypedValue = new TypedValue(1000, $"{key}:{value}");

                    newBuffer.Add(newTypedValue);
                }
                
                obj.XData = newBuffer;
            }
        }

        private void LoadXData()
        {
            _XData = new Dictionary<string, string>();
            Transaction tr = BaseObject.Database.TransactionManager.TopTransaction;
            DBObject obj = tr.GetObject(BaseObject, OpenMode.ForRead);

            ResultBuffer rb = obj.XData;

            if (rb != null)
            {
                foreach (TypedValue tv in rb)
                {
                    string data = tv.Value as string;
                    string[] keyvalue = data.Split(':');
                    if (keyvalue.Length == 2)
                    {
                        _XData.Add(keyvalue[0], keyvalue[1]);
                    }
                }
            }
        }

        public bool HasKey(string key)
        {
            if (_XData == null)
                LoadXData();
                //return false;

            return _XData.ContainsKey(key);
        }

        [XmlIgnore]
        public abstract Point3d Location { get; set; }

        public abstract void Generate();

        [XmlIgnore]
        public abstract double Rotation { get; set; }

        public bool DirtyModified { get; set; }
        public bool DirtyAdded { get; set; }
        public bool DirtyRemoved { get; set; }

	protected Dictionary<string, DrawingObject> SubObjects { get; set; }
	public bool Verified { get; set; }

        protected DrawingObject()
        {
            _database = Application.DocumentManager.MdiActiveDocument.Database;
            _document = Application.DocumentManager.MdiActiveDocument;
            SubObjects = new Dictionary<string, DrawingObject>();
        }

        [Obsolete]
        protected DrawingObject(Database database)
        {
            _database = database;
            SubObjects = new Dictionary<string, DrawingObject>();
        }

        protected DrawingObject(Document document)
        {
            _document = document;
            _database = _document.Database;
            SubObjects = new Dictionary<string, DrawingObject>();
        }

        public virtual void Erase()
        {
            Transaction trans = _database.TransactionManager.TopTransaction;
            Entity ent = (Entity) trans.GetObject(this.BaseObject, OpenMode.ForWrite);
            ent.Erase();

            foreach (DrawingObject drawingObject in SubObjects.Values)
            {
                drawingObject.Erase();
            }
            SubObjects.Clear();
        }

        public virtual Extents3d GetBoundingBox()
        {
            Transaction trans = _database.TransactionManager.TopTransaction;
            Entity ent = (Entity) trans.GetObject(this.BaseObject, OpenMode.ForRead);
            return ent.GeometricExtents;
        }

        public void SetLayer(string name)
        {
            Transaction acTrans = _database.TransactionManager.TopTransaction;
            Entity ent = (Entity)acTrans.GetObject(BaseObject, OpenMode.ForWrite);
            
            string layerName = DataService.Current.GetStore<DocumentStore>(DocumentName).LayerManager.GetLayerName(name);
            ent.Layer = layerName;
        }

        public virtual void ParentUpdated(DrawingObject parent)
        {
        }

        public void DrawOnTop()
        {
            Transaction trans = _document.TransactionManager.TopTransaction;
            BlockTableRecord btr = _document.Database.GetModelSpace(false);
            DrawOrderTable drawOrder = trans.GetObject(btr.DrawOrderTableId, OpenMode.ForWrite) as DrawOrderTable;
            
            ObjectIdCollection ids = new ObjectIdCollection();
            ids.Add(BaseObject);

            drawOrder.MoveToTop(ids);
        }

        public void DrawOnBottom()
        {
            Transaction trans = _document.TransactionManager.TopTransaction;
            BlockTableRecord btr = _document.Database.GetModelSpace(false);
            DrawOrderTable drawOrder = trans.GetObject(btr.DrawOrderTableId, OpenMode.ForWrite) as DrawOrderTable;

            ObjectIdCollection ids = new ObjectIdCollection();
            ids.Add(BaseObject);

            drawOrder.MoveToBottom(ids);
        }
    }
}
