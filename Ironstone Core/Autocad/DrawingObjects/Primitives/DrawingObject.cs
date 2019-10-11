using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Jpp.Ironstone.Core.Autocad
{
    public abstract class DrawingObject
    {
        private DBObject _activeObject;
        private Database _database;

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
                    GenerateBase(_database);
                }

                return _database.GetObjectId(false, new Handle(BaseObjectPtr), 0);
            }
            set
            {
                if (BaseObjectPtr == value.Handle.Value) return;

                //Remove event handles from previous object, change pointer and activate new object.
                UnhookActiveObject();
                BaseObjectPtr = value.Handle.Value;
                CreateActiveObject();
            }
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

        public void CreateActiveObject()
        {
            Transaction acTrans = _database.TransactionManager.TopTransaction;
            _activeObject = acTrans.GetObject(BaseObject, OpenMode.ForWrite);
            _activeObject.Erased += ActiveObject_Erased;
            _activeObject.Modified += ActiveObject_Modified;

            Active = true;
        }

        private void ActiveObject_Modified(object sender, EventArgs e)
        {
            ObjectModified(sender, e);
            DirtyModified = true;
        }

        protected virtual void GenerateBase(Database database)
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
        public string this[string key]
        {
            get
            {
                if (_XData == null)
                {
                    if (_XData.ContainsKey(key))
                        throw new KeyNotFoundException();

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
                            _XData.Add(keyvalue[0], keyvalue[1]);
                        }
                    }
                }

                return _XData[key];
            }
            set
            {
                Transaction tr = BaseObject.Database.TransactionManager.TopTransaction;
                DBObject obj = tr.GetObject(BaseObject, OpenMode.ForRead);

                ResultBuffer rb = obj.XData;
                ResultBuffer newBuffer = new ResultBuffer();

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

                rb.Dispose();
                tr.Commit();
            }
        }

        public bool HasKey(string key)
        {
            if (_XData == null)
                return false;

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

        protected DrawingObject()
        {
            _database = Application.DocumentManager.MdiActiveDocument.Database;
        }

        protected DrawingObject(Database database)
        {
            _database = database;
        }

        public abstract void Erase();
    }
}
