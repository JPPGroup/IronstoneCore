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
        public void VerifyManagerDependenciesSet()
        {
            Assert.Multiple(() =>
            {
                Assert.IsTrue(RunTest<bool>(nameof(VerifyManagerDependenciesSet_HostDocumentResident)));
                Assert.IsTrue(RunTest<bool>(nameof(VerifyManagerDependenciesSet_LogResident)));
            });
        }

        public bool VerifyManagerDependenciesSet_HostDocumentResident()
        {
            try
            {
                return GetManager()?.HostDocument != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool VerifyManagerDependenciesSet_LogResident()
        {
            try
            {
                return GetManager()?.Log != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Test]
        public void VerifyManagerManagedObjects()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyManagerManagedObjectsResident)));
        }

        public bool VerifyManagerManagedObjectsResident()
        {
            try
            {
                var manager = GetManager();
                if (manager == null) return false;
                return manager.ManagedObjects.Count == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Test]
        public void VerifyManagerActiveObjects()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyManagerActiveObjectsResident)));
        }

        public bool VerifyManagerActiveObjectsResident()
        {
            using (Transaction trans = Application.DocumentManager
                .MdiActiveDocument.TransactionManager.StartTransaction())
            {
                try
                {
                    var manager = GetManager();
                    if (manager == null) return false;
                    return manager.ActiveObjects.Count == 0;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        [Test]
        public void VerifyAddManagedObjectActive_ManagedList()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyAddManagedObjectActive_ManagedListResident)));
        }

        public bool VerifyAddManagedObjectActive_ManagedListResident()
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
        public void VerifyAddManagedObjectActive_ActiveList()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyAddManagedObjectActive_ActiveListResident)));
        }

        public bool VerifyAddManagedObjectActive_ActiveListResident()
        {
            using (Transaction trans = Application.DocumentManager
                .MdiActiveDocument.TransactionManager.StartTransaction())
            {
                try
                {
                    var manager = GetManager();
                    if (manager == null) return false;

                    manager.Clear();

                    var objId = Guid.NewGuid();
                    var obj = TestDrawingObject.CreateActiveObject(objId);
                    manager.Add(obj);

                    return manager.ActiveObjects.Count == 1;
                }
                catch (Exception)
                {
                    return false;
                }
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

                return false;
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
        public void VerifyClear_ManagedList()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyClear_ManagedListResident)));
        }

        public bool VerifyClear_ManagedListResident()
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
        public void VerifyClear_ActiveList()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyClear_ActiveListResident)));
        }

        public bool VerifyClear_ActiveListResident()
        {
            try
            {
                var manager = GetManager();
                SetupObjects(manager);

                manager.Clear();

                return manager.ActiveObjects.Count == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Test]
        public void VerifyAllDirty_ManagedList()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyAllDirty_ManagedListResident)));
        }

        public bool VerifyAllDirty_ManagedListResident()
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
        public void VerifyAllDirty_ActiveList()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyAllDirty_ActiveListResident)));
        }

        public bool VerifyAllDirty_ActiveListResident()
        {
            try
            {
                var manager = GetManager();
                SetupObjects(manager);

                manager.AllDirty();

                return manager.ActiveObjects.All(obj => obj.DirtyModified);
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
        public void VerifyUpdateDirty_ManagedList()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyUpdateDirty_ManagedListResident)));
        }

        public bool VerifyUpdateDirty_ManagedListResident()
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

                return manager.ManagedObjects.Count == 2;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Test]
        public void VerifyUpdateDirty_ActiveList()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyUpdateDirty_ActiveListResident)));
        }

        public bool VerifyUpdateDirty_ActiveListResident()
        {
            try
            {
                var manager = GetManager();
                SetupObjects(manager);

                var obj = manager.ActiveObjects.First();

                EraseObject(obj);

                if (manager.ManagedObjects.Count != 2) return false;
                if (manager.ManagedObjects.Count(o => o.DirtyRemoved) != 1) return false;
                if (manager.ManagedObjects.Count(o => o.DirtyModified) != 1) return false;

                manager.UpdateDirty();

                return manager.ActiveObjects.Count == 1;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Test]
        public void VerifyUpdateAll_ManagedList()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyUpdateAll_ManagedListResident)));
        }

        public bool VerifyUpdateAll_ManagedListResident()
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

                return manager.ManagedObjects.Count == 2;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Test]
        public void VerifyUpdateAll_ActiveList()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyUpdateAll_ActiveListResident)));
        }

        public bool VerifyUpdateAll_ActiveListResident()
        {
            try
            {
                var manager = GetManager();
                SetupObjects(manager);

                var obj = manager.ActiveObjects.First();

                EraseObject(obj);

                if (manager.ManagedObjects.Count != 2) return false;
                if (manager.ManagedObjects.Count(o => o.DirtyRemoved) != 1) return false;
                if (manager.ManagedObjects.Count(o => o.DirtyModified) != 1) return false;

                manager.UpdateAll();

                return manager.ActiveObjects.Count == 1;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Test]
        public void VerifyUndoRegenerate()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyUndoRegenerateResident)));
        }

        public bool VerifyUndoRegenerateResident()
        {
            try
            {
                using (var trans = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
                {
                    var ed = Application.DocumentManager.MdiActiveDocument.Editor;

                    var manager = GetManager();
                    if (manager == null) return false;

                    manager.Clear();
                    trans.Commit();

                    ClearDrawingObjects();

                    ed.Command("_regen");
                    var count = GetObjectCount();

                    ed.Command("_undo", "MARK");

                    ed.Command("_circle", "0,0", 10);
                    if (GetObjectCount() != count + 1) return false;

                    ed.Command("_undo", "BACK");
                    return GetObjectCount() == count;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Test]
        public void VerifyUndo_ManagedList()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyUndo_ManagedListResident)));
        }

        public bool VerifyUndo_ManagedListResident()
        {
            try
            {
                var ed = Application.DocumentManager.MdiActiveDocument.Editor;
                var manager = GetManager();
                SetupObjects(manager);

                if (manager.ManagedObjects.Count != 2) return false;

                ed.Command("_undo", "MARK");

                var obj = manager.ManagedObjects.First();
                EraseObject(obj);

                ed.Command("_regen");

                if (manager.ManagedObjects.Count != 2) return false;

                ed.Command("_undo", "BACK");

                return manager.ManagedObjects.Count == 2;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Test]
        public void VerifyUndo_ActiveList()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyUndo_ActiveListResident)));
        }

        public bool VerifyUndo_ActiveListResident()
        {
            try
            {
                var ed = Application.DocumentManager.MdiActiveDocument.Editor;
                var manager = GetManager();
                SetupObjects(manager);

                if (manager.ActiveObjects.Count != 2) return false;

                ed.Command("_undo", "MARK");

                var obj = manager.ActiveObjects.First();
                EraseObject(obj);

                ed.Command("_regen");

                if (manager.ActiveObjects.Count != 1) return false;

                ed.Command("_undo", "BACK");

                return manager.ActiveObjects.Count == 2;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static TestDrawingObjectManager GetManager()
        {
            using (var trans = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
            {
                var ds = DataService.Current;
                ds.InvalidateStoreTypes();

                var store = ds.GetStore<TestDocumentStore>(Application.DocumentManager.MdiActiveDocument.Name);
                trans.Commit();
                
                return store?.GetManager<TestDrawingObjectManager>();
            }
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

        [Test]
        public void VerifyLayerCreation()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyLayerCreationResident)));
        }

        public bool VerifyLayerCreationResident()
        {
            using (Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager
                .StartTransaction())
            {
                DataService ds = DataService.Current;
                ds._stores.Remove(Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager
                    .MdiActiveDocument.Name);
                GetManager();

                trans.Commit();
            }

            using (Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager
                .StartTransaction())
            {
                return Application.DocumentManager.MdiActiveDocument.Database
                           .GetLayer("TestDrawingObjectManagerLayer") !=
                       null;
            }
        }
    }
}
