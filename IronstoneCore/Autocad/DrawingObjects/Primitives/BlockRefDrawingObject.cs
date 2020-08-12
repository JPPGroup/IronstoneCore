using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Jpp.Ironstone.Core.Autocad
{
    public class BlockRefDrawingObject : DrawingObject
    {
        protected BlockRefDrawingObject() : base()
        {
            _properties = new Dictionary<string, object>();
        }

        private Dictionary<string, object> _properties;

        public BlockRefDrawingObject(Document doc, BlockReference reference) : base(doc)
        {
            BaseObject = reference.ObjectId;
            _properties = new Dictionary<string, object>();
        }

        public string BlockName
        {
            get
            {
                if(_cachedBlockName == null)
                    UpdateCachedFields();

                return _cachedBlockName;
            }
        }

        public string _cachedBlockName;

        protected void GetProperties()
        {
            Transaction acTrans = _database.TransactionManager.TopTransaction;
            BlockReference reference = (BlockReference) acTrans.GetObject(BaseObject, OpenMode.ForRead);

            if (reference.IsDynamicBlock)
            {
                DynamicBlockReferencePropertyCollection pc = reference.DynamicBlockReferencePropertyCollection;
                foreach (DynamicBlockReferenceProperty dynamicBlockReferenceProperty in pc)
                {
                    if (_properties.ContainsKey(dynamicBlockReferenceProperty.PropertyName))
                    {
                        _properties[dynamicBlockReferenceProperty.PropertyName] = dynamicBlockReferenceProperty.Value;
                    }
                    else
                    {
                        _properties.Add(dynamicBlockReferenceProperty.PropertyName,
                            dynamicBlockReferenceProperty.Value);
                    }
                }

                foreach (ObjectId attId in reference.AttributeCollection)
                {
                    AttributeReference attRef = (AttributeReference) acTrans.GetObject(attId, OpenMode.ForRead);
                    if (_properties.ContainsKey(attRef.Tag))
                    {
                        _properties[attRef.Tag] = attRef.TextString;
                    }
                    else
                    {
                        _properties.Add(attRef.Tag, attRef.TextString);
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        protected void SetPropertyOnObject(string name, object value)
        {
            Transaction acTrans = _database.TransactionManager.TopTransaction;
            BlockReference reference = (BlockReference) acTrans.GetObject(BaseObject, OpenMode.ForWrite);

            if (reference.IsDynamicBlock)
            {
                DynamicBlockReferencePropertyCollection pc = reference.DynamicBlockReferencePropertyCollection;
                foreach (DynamicBlockReferenceProperty dynamicBlockReferenceProperty in pc)
                {
                    if (dynamicBlockReferenceProperty.PropertyName == name)
                    {
                        dynamicBlockReferenceProperty.Value = value;
                        return;
                    }
                }

                foreach (ObjectId attId in reference.AttributeCollection)
                {
                    AttributeReference attRef = (AttributeReference) acTrans.GetObject(attId, OpenMode.ForRead);
                    if (attRef.Tag == name)
                    {
                        attRef.UpgradeOpen();
                        attRef.TextString = (string)value;
                        return;
                    }
                }

                throw new InvalidOperationException("Property not found");
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public T GetProperty<T>(string name)
        {
            if(!_properties.ContainsKey(name))
                GetProperties();

            if (!_properties.ContainsKey(name))
                return default(T);

            return (T) _properties[name];
        }

        public void SetProperty(string name, object value)
        {
            SetPropertyOnObject(name, value);

            if (!_properties.ContainsKey(name))
                _properties[name] = value;
        }

        protected override void ObjectModified(object sender, EventArgs e)
        {
            // TODO: These need to be renabled
            //UpdateCachedFields();
            //GetProperties();
        }

        protected override void ObjectErased(object sender, ObjectErasedEventArgs e)
        {
            
        }

        public override Point3d Location { get; set; }
        public override void Generate()
        {
            throw new NotImplementedException();
        }

        public override double Rotation { get; set; }
        public override void Erase()
        {
            throw new NotImplementedException();
        }

        private void UpdateCachedFields()
        {
            Transaction acTrans = _database.TransactionManager.TopTransaction;
            DBObject res = acTrans.GetObject(BaseObject, OpenMode.ForWrite);
            BlockReference obj = (BlockReference) res;

            _cachedBlockName = obj.IsDynamicBlock ? ((BlockTableRecord) obj.DynamicBlockTableRecord.GetObject(OpenMode.ForRead)).Name : obj.Name;
        }


        // TODO: Add test
        public BlockDrawingObject GetBlock()
        {
            BlockDrawingObject blockDrawingObject = new BlockDrawingObject();
            blockDrawingObject.BaseObject = _database.GetBlockDefinition(BlockName).ObjectId;
            return blockDrawingObject;
        }

        public static BlockRefDrawingObject Create(Database target, Point3d insertionPoint, BlockDrawingObject sourceBlock)
        {
            ObjectId newRefId;
            Transaction trans = target.TransactionManager.TopTransaction;
            BlockReference acadReference = new BlockReference(insertionPoint, sourceBlock.BaseObject);
            newRefId = target.GetModelSpace(true).AppendEntity(acadReference);
            trans.AddNewlyCreatedDBObject(acadReference, true);
            
            // TODO: Figure out why the belwo throws an exception when the modified handler is active
            // Exception is thrown when the object is attempted to be opened, during the modified event handler
            BlockRefDrawingObject newRef = new BlockRefDrawingObject();            
            newRef.BaseObject = newRefId;
            return newRef;
        }

        public DBObjectCollection Explode(bool addToModelSpace = false)
        {
            Transaction trans = this._document.Database.TransactionManager.TopTransaction;

            DBObjectCollection collection = new DBObjectCollection();

            BlockReference reference = (BlockReference)trans.GetObject(this.BaseObject, OpenMode.ForRead);
            reference.Explode(collection);

            if (addToModelSpace)
            {
                foreach (DBObject dbObject in collection)
                {
                    Entity ent = (Entity)dbObject;
                    BlockTableRecord btr = _document.Database.GetModelSpace(true);
                    btr.AppendEntity(ent);
                    trans.AddNewlyCreatedDBObject(ent, true);
                }
            }

            return collection;
        }
    }
}
