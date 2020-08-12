using System;
using System.Drawing;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.Core.Tests.TestObjects
{
    [Layer(Name = "TestDrawingObjectLayer")]
    //Requires AcDbMgd to be copied local to run
    public class TestDrawingObject : DrawingObject
    {
        public Guid BaseGuid { get; set; }

        protected override void ObjectModified(object sender, EventArgs e)
        {
            //Do nothing...
        }

        protected override void ObjectErased(object sender, ObjectErasedEventArgs e)
        {
            //Do nothing...
        }

        public override Point3d Location { get; set; }
        public override void Generate()
        {
            throw new NotImplementedException();
        }

        public override double Rotation { get; set; }
        public override void Erase()
        {
            //Do nothing...
        }
        
        public static TestDrawingObject CreateActiveObject(Guid id)
        {
            var obj = CreateNonActiveObject(id);

            ObjectId objId;
            var db = Application.DocumentManager.MdiActiveDocument.Database;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                var line = new Line(new Point3d(0, 0, 0), new Point3d(3, 3, 0));

                objId = blockTableRecord.AppendEntity(line);
                tr.AddNewlyCreatedDBObject(line, true);

                tr.Commit();
            }

            using (var tr = db.TransactionManager.StartTransaction())
            {
                obj.BaseObject = objId;
                tr.Commit();
            }

            return obj;
        }

        public static TestDrawingObject CreateNonActiveObject(Guid id)
        {
            return new TestDrawingObject { BaseGuid = id };
        }

    }
}
