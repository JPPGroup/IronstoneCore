﻿using System;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.ServiceInterfaces;

namespace Jpp.Ironstone.Core.Autocad
{
    public class BlockDrawingObject : DrawingObject
    {
        public const string TEMPLATE_ID_KEY = "TemplateId";
        public const string DOCSTORE_TYPE_KEY = "DocstoreType";
        public const string MANAGER_TYPE_KEY = "ManagerType";
        
        protected override void ObjectModified(object sender, EventArgs e)
        {
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

        public string Name
        {
            get
            {
                Transaction trans = _database.TransactionManager.TopTransaction;
                BlockTableRecord record = (BlockTableRecord)trans.GetObject(BaseObject, OpenMode.ForRead);
                return record.Name;
            }
        }

        public BlockDrawingObject() : base()
        {
        }

        public BlockDrawingObject(Database database) : base(database)
        {
        }
        
        public BlockDrawingObject(Document document) : base(document)
        {
        }

        public void AddEntity(Entity entity)
        {
            Transaction trans = Document.TransactionManager.TopTransaction;
            BlockTableRecord block =  (BlockTableRecord) trans.GetObject(BaseObject, OpenMode.ForWrite);

            block.AppendEntity(entity);
            trans.AddNewlyCreatedDBObject(entity, true);
        }

        public TemplateDrawingObject ConvertToTemplate()
        {
            this[TEMPLATE_ID_KEY] = Guid.NewGuid().ToString();
            TemplateDrawingObject result = new TemplateDrawingObject();
            result.BaseObject = this.BaseObject;

            //Determine if a managed object
            foreach (DocumentStore ds in DataService.Current._stores[Document.Name].Values)
            {
                foreach (var manager in ds.Managers)
                {
                    if (manager.GetAllDrawingObjects().Contains(this))
                    {
                        this[MANAGER_TYPE_KEY] = manager.GetType().FullName;
                        this[DOCSTORE_TYPE_KEY] = ds.GetType().FullName;
                    }
                }
            }

            return result;
        }

        public BlockDrawingObject TransferToDocument(Document targetDocument)
        {
            Transaction sourceTrans = this._database.TransactionManager.TopTransaction;
            Transaction destinatiopnTrans = targetDocument.Database.TransactionManager.TopTransaction;
            BlockTableRecord sourceBlockTableRecord = sourceTrans.GetObject(this.BaseObject, OpenMode.ForRead) as BlockTableRecord;
            ObjectIdCollection sourceObjects = new ObjectIdCollection();
            /*foreach (ObjectId objectId in sourceBlockTableRecord)
            {
                sourceObjects.Add(objectId);
            }*/
            sourceObjects.Add(sourceBlockTableRecord.ObjectId);
            IdMapping mapping = new IdMapping();
            
            // TODO: Confirm ignore is correct option
            _database.WblockCloneObjects(sourceObjects, targetDocument.Database.BlockTableId, mapping, DuplicateRecordCloning.Ignore, false);

            BlockDrawingObject blockDrawingObject = new BlockDrawingObject(targetDocument);
            blockDrawingObject.BaseObject = mapping.Lookup(sourceBlockTableRecord.ObjectId).Value;
            return blockDrawingObject;
        }

        public static BlockDrawingObject Create(Document target, string blockName)
        {
            Transaction trans = target.TransactionManager.TopTransaction;
            BlockTable bt = (BlockTable)trans.GetObject(target.Database.BlockTableId, OpenMode.ForRead);

            SymbolUtilityServices.ValidateSymbolName(blockName, false);
            if(bt.Has(blockName))
                throw  new InvalidOperationException("Block name exists");

            BlockTableRecord btr = new BlockTableRecord();
            btr.Name = blockName;

            bt.UpgradeOpen();
            ObjectId btrId = bt.Add(btr);
            trans.AddNewlyCreatedDBObject(btr, true);

            BlockDrawingObject blockDrawingObject = new BlockDrawingObject(target);
            blockDrawingObject.BaseObject = btrId;
            return blockDrawingObject;
        }

        public static BlockDrawingObject GetExisting(Document target, string blockName)
        {
            BlockTableRecord btr = target.Database.GetBlockDefinition(blockName);

            BlockDrawingObject blockDrawingObject = new BlockDrawingObject(target);
            blockDrawingObject.BaseObject = btr.ObjectId;
            return blockDrawingObject;
        }
    }
}
