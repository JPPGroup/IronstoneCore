using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace Jpp.Ironstone.Core.Autocad
{
    public abstract class BlockRefDrawingObject : DrawingObject
    {
        protected BlockRefDrawingObject() : base()
        { }

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

        public T GetProperty<T>(string name)
        {
            if(!_properties.ContainsKey(name))
                GetProperties();

            return (T) _properties[name];
        }

        protected override void ObjectModified(object sender, EventArgs e)
        {
            UpdateCachedFields();
            GetProperties();
        }

        private void UpdateCachedFields()
        {
            Transaction acTrans = _database.TransactionManager.TopTransaction;
            BlockReference obj = (BlockReference) acTrans.GetObject(BaseObject, OpenMode.ForRead);

            _cachedBlockName = obj.IsDynamicBlock ? ((BlockTableRecord)obj.DynamicBlockTableRecord.GetObject(OpenMode.ForRead)).Name : obj.Name;
        }
    }
}
