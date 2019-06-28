using System;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Tests.TestObjects;
using NUnit.Framework;

namespace Jpp.Ironstone.Core.Tests.Autocad
{
    [TestFixture]
    public class DrawingObjectTests : IronstoneTestFixture
    {
        public DrawingObjectTests() : base(Assembly.GetExecutingAssembly(), typeof(DrawingObjectTests)) { }

        [Test]
        public void VerifyTestSetBase()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyTestSetBaseResident)));
        }

        public bool VerifyTestSetBaseResident()
        {
            try
            {
                var obj = new TestDrawingObject();

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

                return obj.Active;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Test]
        public void VerifyTestGetBaseNoPointer()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyTestGetBaseNoPointerResident)));
        }

        public bool VerifyTestGetBaseNoPointerResident()
        {
            try
            {
                var obj = new TestDrawingObject();
                if (obj.BaseObjectPtr != 0) return false;

                var unused = obj.BaseObject;
                return false;
            }
            catch (NullReferenceException e)
            {
                return e.Message == "No base object has been linked";
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Test]
        public void VerifyTestGetBaseWithPointer()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyTestGetBaseWithPointerResident)));
        }

        public bool VerifyTestGetBaseWithPointerResident()
        {
            try
            {
                var obj = new TestDrawingObject();

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

                return obj.BaseObjectPtr == objId.Handle.Value;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Test]
        public void VerifyTestEraseObject()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyTestEraseObjectResident)));
        }

        public bool VerifyTestEraseObjectResident()
        {
            try
            {
                var obj = TestDrawingObject.CreateActiveObject(Guid.NewGuid());

                if (obj.DirtyRemoved) return false;

                var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
                using (var trans = acCurDb.TransactionManager.StartTransaction())
                {
                    var entity = trans.GetObject(obj.BaseObject, OpenMode.ForWrite) as Entity;
                    entity?.Erase();
                    trans.Commit();
                }

                return obj.DirtyRemoved;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Test]
        public void VerifyTestModifyObject()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyTestModifyObjectResident)));
        }

        public bool VerifyTestModifyObjectResident()
        {
            try
            {
                var obj = TestDrawingObject.CreateActiveObject(Guid.NewGuid());

                if (obj.DirtyModified) return false;

                var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
                using (var trans = acCurDb.TransactionManager.StartTransaction())
                {
                    var curve = trans.GetObject(obj.BaseObject, OpenMode.ForWrite) as Curve;
                    if (curve != null) curve.StartPoint = new Point3d(1, 1, 1);
                    trans.Commit();
                }

                return obj.DirtyModified;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
