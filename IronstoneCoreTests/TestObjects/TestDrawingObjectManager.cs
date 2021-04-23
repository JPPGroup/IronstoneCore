using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Jpp.Ironstone.Core.Tests.TestObjects
{
    [Layer(Name = "TestDrawingObjectManagerLayer")]
    public class TestDrawingObjectManager : AbstractDrawingObjectManager<TestDrawingObject>
    {
        public PersistentObjectIdCollection ObjectCollection { get; set; }

        public TestDrawingObjectManager(Document document, ILogger<CoreExtensionApplication> log, IConfiguration settings) : base(document, log, settings)
        {
            ObjectCollection = new PersistentObjectIdCollection();
        }

        private TestDrawingObjectManager()
        {
            ObjectCollection = new PersistentObjectIdCollection();
        }

        public override void UpdateDirty()
        {
            base.UpdateDirty();
            GenerateSimpleObject();
        }

        public override void UpdateAll()
        {
            base.UpdateAll();
            GenerateSimpleObject();
        }

        private void GenerateSimpleObject()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                foreach (ObjectId obj in ObjectCollection.Collection)
                {
                    if (!obj.IsErased) acTrans.GetObject(obj, OpenMode.ForWrite).Erase();
                }

                ObjectCollection.Clear();

                CreateLine();

                acTrans.Commit();
            }
        }

        private void CreateLine()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;
            var topTrans = acCurDb.TransactionManager.TopTransaction;

            var blockTable = (BlockTable)topTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead);
            var blockTableRecord = (BlockTableRecord)topTrans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            var line = new Line(new Point3d(0, 0, 0), new Point3d(0, 10, 0));

            ObjectCollection.Add(blockTableRecord.AppendEntity(line));
            topTrans.AddNewlyCreatedDBObject(line, true);
        }
    }
}
