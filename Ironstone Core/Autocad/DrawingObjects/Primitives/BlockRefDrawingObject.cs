using System;
using System.Collections.Generic;
using System.Drawing;
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

        public BlockRefDrawingObject(BlockReference reference) : base()
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
            UpdateCachedFields();
            GetProperties();
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
            BlockReference obj = (BlockReference) acTrans.GetObject(BaseObject, OpenMode.ForRead);

            _cachedBlockName = obj.IsDynamicBlock ? ((BlockTableRecord)obj.DynamicBlockTableRecord.GetObject(OpenMode.ForRead)).Name : obj.Name;
        }
    }
}
