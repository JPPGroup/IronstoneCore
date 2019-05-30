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
        public void VerifyTestManagerConstructor()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyTestManagerConstructorResident)));
        }

        public bool VerifyTestManagerConstructorResident()
        {
            return Activator.CreateInstance(typeof(TestDrawingObjectManager), true) is TestDrawingObjectManager;
        }

        [Test]
        public void VerifyTestManagerExists()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyTestManagerExistsResident)));
        }

        public bool VerifyTestManagerExistsResident()
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
        public void VerifyTestAddManagedObjectActive()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyTestAddManagedObjectActiveResident)));
        }

        public bool VerifyTestAddManagedObjectActiveResident()
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
        public void VerifyTestAddManagedObjectNonActive()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyTestAddManagedObjectNonActiveResident)));
        }

        public bool VerifyTestAddManagedObjectNonActiveResident()
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
        public void VerifyTestClear()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyTestClearResident)));
        }

        public bool VerifyTestClearResident()
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
            Assert.IsTrue(RunTest<bool>(nameof(VerifyTestAllDirtyResident)));
        }

        public bool VerifyTestAllDirtyResident()
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
        public void VerifyTestRemovedErased()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyTestRemovedErasedResident)));
        }

        public bool VerifyTestRemovedErasedResident()
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
        public void VerifyTestUpdateDirty()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyTestUpdateDirtyResident)));
        }

        public bool VerifyTestUpdateDirtyResident()
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
        public void VerifyTestUpdateAll()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyTestUpdateAllResident)));
        }

        public bool VerifyTestUpdateAllResident()
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
    }
}
