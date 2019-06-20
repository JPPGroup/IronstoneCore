using System;
using System.Linq;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Core.Tests.TestObjects;
using NUnit.Framework;

namespace Jpp.Ironstone.Core.Tests.Autocad
{
    [TestFixture]
    public class DrawingObjectManagerTests : IronstoneTestFixture
    {
        public DrawingObjectManagerTests() : base(Assembly.GetExecutingAssembly(), typeof(DrawingObjectManagerTests)) { }

        [Test]
        public void VerifyManagerConstructor()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyManagerConstructorResident)));
        }

        public bool VerifyManagerConstructorResident()
        {
            return Activator.CreateInstance(typeof(TestDrawingObjectManager), true) is TestDrawingObjectManager;
        }

        [Test]
        public void VerifyManagerExists()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyManagerExistsResident)));
        }

        public bool VerifyManagerExistsResident()
        {
            try
            {
                return GetManager() != null;
            }
            catch (Exception)
            {
                return false;
            }
        } 

        [Test]
        public void VerifyAddManagedObjectActive()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyAddManagedObjectActiveResident)));
        }

        public bool VerifyAddManagedObjectActiveResident()
        {
            try
            {
                var manager = GetManager();
                if (manager == null) return false;

                manager.Clear();

                var objId = Guid.NewGuid();
                var obj = TestDrawingObject.CreateActiveObject(objId);
                manager.Add(obj);

                return manager.ManagedObjects.Count == 1;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Test]
        public void VerifyAddManagedObjectNonActive()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyAddManagedObjectNonActiveResident)));
        }

        public bool VerifyAddManagedObjectNonActiveResident()
        {
            try
            {
                var manager = GetManager();
                if (manager == null) return false;

                manager.Clear();

                var objId = Guid.NewGuid();
                var obj = TestDrawingObject.CreateNonActiveObject(objId);
                manager.Add(obj);

                return manager.ManagedObjects.Count == 0;
            }
            catch (ArgumentException e)
            {
                return e.Message == "Drawing object not active.";
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Test]
        public void VerifyClear()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyClearResident)));
        }

        public bool VerifyClearResident()
        {
            try
            {
                var manager = GetManager();
                SetupObjects(manager);

                manager.Clear();

                return manager.ManagedObjects.Count == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Test]
        public void VerifyAllDirtyClear()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyAllDirtyResident)));
        }

        public bool VerifyAllDirtyResident()
        {
            try
            {
                var manager = GetManager();
                SetupObjects(manager);

                manager.AllDirty();

                return manager.ManagedObjects.All(obj => obj.DirtyModified);
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Test]
        public void VerifyRemovedErased()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyRemovedErasedResident)));
        }

        public bool VerifyRemovedErasedResident()
        {
            try
            {
                var manager = GetManager();
                SetupObjects(manager);

                var obj = manager.ManagedObjects.First();

                EraseObject(obj);

                if (manager.ManagedObjects.Count != 2) return false;
                if (manager.ManagedObjects.Count(o => o.DirtyRemoved) != 1) return false;
                if (manager.ManagedObjects.Count(o => o.DirtyModified) != 1) return false;

                manager.RemoveErased();

                return manager.ManagedObjects.Count == 1;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Test]
        public void VerifyUpdateDirty()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyUpdateDirtyResident)));
        }

        public bool VerifyUpdateDirtyResident()
        {
            try
            {
                var manager = GetManager();
                SetupObjects(manager);

                var obj = manager.ManagedObjects.First();

                EraseObject(obj);

                if (manager.ManagedObjects.Count != 2) return false;
                if (manager.ManagedObjects.Count(o => o.DirtyRemoved) != 1) return false;
                if (manager.ManagedObjects.Count(o => o.DirtyModified) != 1) return false;

                manager.UpdateDirty();

                return manager.ManagedObjects.Count == 1;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Test]
        public void VerifyUpdateAll()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyUpdateAllResident)));
        }

        public bool VerifyUpdateAllResident()
        {
            try
            {
                var manager = GetManager();
                SetupObjects(manager);

                var obj = manager.ManagedObjects.First();

                EraseObject(obj);

                if (manager.ManagedObjects.Count != 2) return false;
                if (manager.ManagedObjects.Count(o => o.DirtyRemoved) != 1) return false;
                if (manager.ManagedObjects.Count(o => o.DirtyModified) != 1) return false;

                manager.UpdateAll();

                return manager.ManagedObjects.Count == 1;
            }
            catch (Exception)
            {
                return false;
            }
        }
        [Test]
        public void VerifyUndoAction()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyUndoActionResident)));
        }

        public bool VerifyUndoActionResident()
        {
            try
            {
                ClearDrawingObjects();

                var ed = Application.DocumentManager.MdiActiveDocument.Editor;
                var manager = GetManager();
                
                if (manager == null) return false;
                manager.Clear();

                ed.Command("_regen");
                if (GetObjectCount() != 1) return false;

                ed.Command("_undo", "MARK");

                ed.Command("_circle", "0,0", 10);
                if (GetObjectCount() != 2) return false;

                ed.Command("_undo", "BACK");
                return GetObjectCount() == 1;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static TestDrawingObjectManager GetManager()
        {
            var ds = DataService.Current;
            ds.InvalidateStoreTypes();

            var store = ds.GetStore<TestDocumentStore>(Application.DocumentManager.MdiActiveDocument.Name);
            return store?.GetManager<TestDrawingObjectManager>();
        }

        private static void SetupObjects(TestDrawingObjectManager manager)
        {
            if (manager == null) throw new ArgumentException("No manager.");

            manager.Clear();

            if (manager.ManagedObjects.Count != 0) throw new ArgumentException("Objects not cleared.");

            var objId1 = Guid.NewGuid();
            var objId2 = Guid.NewGuid();

            var obj1 = TestDrawingObject.CreateActiveObject(objId1);
            var obj2 = TestDrawingObject.CreateActiveObject(objId2);

            manager.Add(obj1);
            manager.Add(obj2);

            if (manager.ManagedObjects.Count != 2) throw new ArgumentException("Incorrect count of objects.");
        }

        private static void EraseObject(DrawingObject obj)
        {
            var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
            using (var trans = acCurDb.TransactionManager.StartTransaction())
            {
                var entity = trans.GetObject(obj.BaseObject, OpenMode.ForWrite) as Entity;
                entity?.Erase();
                trans.Commit();
            }
        }

        private static int GetObjectCount()
        {
            var db = Application.DocumentManager.MdiActiveDocument.Database;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                return blockTableRecord.Cast<ObjectId>().Count();
            }
        }

        private static void ClearDrawingObjects()
        {
            var db = Application.DocumentManager.MdiActiveDocument.Database;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                foreach (var objectId in blockTableRecord.Cast<ObjectId>())
                {
                    var entity = tr.GetObject(objectId, OpenMode.ForWrite) as Entity;
                    entity?.Erase();
          
                }
                tr.Commit();
            }
        }
    }
}
