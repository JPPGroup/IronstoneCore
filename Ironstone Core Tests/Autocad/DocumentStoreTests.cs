using System;
using System.Diagnostics;
using System.IO;
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
    public class DocumentStoreTests : IronstoneTestFixture
    {
        public DocumentStoreTests() : base(Assembly.GetExecutingAssembly(), typeof(DocumentStoreTests)) { }

        [Test]
        public void VerifyDocumentStoreSaveCommand()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyDocumentStoreSaveCommandResident)));
            string testFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Test1.dwg");
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
            }
        }

        public bool VerifyDocumentStoreSaveCommandResident()
        {
            using (Transaction trans = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
            {
                try
                {
                    var ds = DataService.Current;
                    ds.InvalidateStoreTypes();

                    var store = ds.GetStore<TestDocumentStore>(Application.DocumentManager.MdiActiveDocument.Name);
                    if (store.SaveTestProperty) return false;

                    store.SaveTestProperty = true;

                    var ed = Application.DocumentManager.MdiActiveDocument.Editor;
                    ed.Command("_qsave", "Test1.dwg");
                    store = ds.GetStore<TestDocumentStore>(Application.DocumentManager.MdiActiveDocument.Name);

                    trans.Commit();

                    return store.SaveTestProperty;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        [Test]
        public void VerifyTestManagerSaveLoadManagedObject()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyTestManagerSaveLoadManagedObjectResident)));
        }

        public bool VerifyTestManagerSaveLoadManagedObjectResident()
        {
            using (Transaction trans = Application.DocumentManager
                .MdiActiveDocument.TransactionManager.StartTransaction())
            {
                try
                {
                    var ds = DataService.Current;
                    ds.InvalidateStoreTypes();

                    var store = ds.GetStore<TestDocumentStore>(Application.DocumentManager.MdiActiveDocument.Name);
                    var manager = store?.GetManager<TestDrawingObjectManager>();

                    if (manager == null) return false;

                    var objId = Guid.NewGuid();
                    var obj = TestDrawingObject.CreateActiveObject(objId);
                    manager.Add(obj);

                    store.SaveWrapper();
                    store.LoadWrapper();

                    manager = store.GetManager<TestDrawingObjectManager>();
                    if (manager == null) return false;

                    var objCount = manager.ManagedObjects.Count;
                    if (objCount != 1) return false;

                    return manager.ManagedObjects.First().BaseGuid == objId;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        [Test]
        public void VerifyLayerCreationWithTransaction()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyLayerCreationResident), true));
        }

        [Test]
        public void VerifyLayerCreationWithoutTransaction()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyLayerCreationResident), false));
        }

        public bool VerifyLayerCreationResident(bool useTransaction)
        {
            if (useTransaction)
            {
                using (Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
                {
                    DataService ds = DataService.Current;
                    ds.InvalidateStoreTypes();
                    var store = ds.GetStore<TestDocumentStore>(Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager
                        .MdiActiveDocument.Name);

                    trans.Commit();
                }
            }
            else
            {
                DataService ds = DataService.Current;
                ds.InvalidateStoreTypes();
                var store = ds.GetStore<TestDocumentStore>(Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager
                    .MdiActiveDocument.Name);
            }
            
            using (Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager
                .StartTransaction())
            {
                return Application.DocumentManager.MdiActiveDocument.Database
                           .GetLayer("TestDocumentStoreLayer") !=
                       null;
            }
        }

        [Test]
        public void VerifyLayerUnLockUnfreezeFalseLayerValidUpdateAll()
        {
            const bool shouldUpdateAll = true;
            const bool isFrozen = false;
            const bool isLocked = false;

            Assert.IsTrue(RunTest<bool>(nameof(VerifyLayerUnLockUnfreezeFalseLayerUpdateAllResident), new[] { shouldUpdateAll , isFrozen, isLocked }));
        }

        [Test]
        public void VerifyLayerUnLockUnfreezeFalseLayerInvalidLockedUpdateAll()
        {
            const bool shouldUpdateAll = false;
            const bool isFrozen = false;
            const bool isLocked = true;

            Assert.IsTrue(RunTest<bool>(nameof(VerifyLayerUnLockUnfreezeFalseLayerUpdateAllResident), new[] { shouldUpdateAll, isFrozen, isLocked }));
        }

        [Test]
        public void VerifyLayerUnLockUnfreezeFalseLayerInvalidFrozenUpdateAll()
        {
            const bool shouldUpdateAll = false;
            const bool isFrozen = true;
            const bool isLocked = false;

            Assert.IsTrue(RunTest<bool>(nameof(VerifyLayerUnLockUnfreezeFalseLayerUpdateAllResident), new[] { shouldUpdateAll, isFrozen, isLocked }));
        }

        [Test]
        public void VerifyLayerUnLockUnfreezeFalseLayerInvalidLockedFrozenUpdateAll()
        {
            const bool shouldUpdateAll = false;
            const bool isFrozen = true;
            const bool isLocked = true;

            Assert.IsTrue(RunTest<bool>(nameof(VerifyLayerUnLockUnfreezeFalseLayerUpdateAllResident), new[] { shouldUpdateAll, isFrozen, isLocked }));
        }

        [Test]
        public void VerifyLayerUnLockUnfreezeFalseLayerValidUpdateDirty()
        {
            const bool shouldUpdateDirty = true;
            const bool isFrozen = false;
            const bool isLocked = false;

            Assert.IsTrue(RunTest<bool>(nameof(VerifyLayerUnLockUnfreezeFalseLayerUpdateDirtyResident), new[] { shouldUpdateDirty, isFrozen, isLocked }));
        }

        [Test]
        public void VerifyLayerUnLockUnfreezeFalseLayerInvalidLockedUpdateDirty()
        {
            const bool shouldUpdateDirty = false;
            const bool isFrozen = false;
            const bool isLocked = true;

            Assert.IsTrue(RunTest<bool>(nameof(VerifyLayerUnLockUnfreezeFalseLayerUpdateDirtyResident), new[] { shouldUpdateDirty, isFrozen, isLocked }));
        }

        [Test]
        public void VerifyLayerUnLockUnfreezeFalseLayerInvalidFrozenUpdateDirty()
        {
            const bool shouldUpdateDirty = false;
            const bool isFrozen = true;
            const bool isLocked = false;

            Assert.IsTrue(RunTest<bool>(nameof(VerifyLayerUnLockUnfreezeFalseLayerUpdateDirtyResident), new[] { shouldUpdateDirty, isFrozen, isLocked }));
        }

        [Test]
        public void VerifyLayerUnLockUnfreezeFalseLayerInvalidLockedFrozenUpdateDirty()
        {
            const bool shouldUpdateDirty = false;
            const bool isFrozen = true;
            const bool isLocked = true;

            Assert.IsTrue(RunTest<bool>(nameof(VerifyLayerUnLockUnfreezeFalseLayerUpdateDirtyResident), new[] { shouldUpdateDirty, isFrozen, isLocked }));
        }

        [Test]
        public void VerifyLayerUnLockUnfreezeTrueLayerLockedFrozenUpdateDirty()
        {
            const bool isFrozen = true;
            const bool isLocked = true;

            Assert.IsTrue(RunTest<bool>(nameof(VerifyLayerUnLockUnfreezeTrueLayerUpdateDirtyResident), new[] { isFrozen, isLocked }));
        }

        [Test]
        public void VerifyLayerUnLockUnfreezeTrueLayerFrozenUpdateDirty()
        {
            const bool isFrozen = true;
            const bool isLocked = true;

            Assert.IsTrue(RunTest<bool>(nameof(VerifyLayerUnLockUnfreezeTrueLayerUpdateDirtyResident), new[] { isFrozen, isLocked }));
        }

        [Test]
        public void VerifyLayerUnLockUnfreezeTrueLayerLockedUpdateDirty()
        {
            const bool isFrozen = true;
            const bool isLocked = true;

            Assert.IsTrue(RunTest<bool>(nameof(VerifyLayerUnLockUnfreezeTrueLayerUpdateDirtyResident), new[] { isFrozen, isLocked }));
        }

        [Test]
        public void VerifyLayerUnLockUnfreezeTrueLayerUpdateDirty()
        {
            const bool isFrozen = true;
            const bool isLocked = true;

            Assert.IsTrue(RunTest<bool>(nameof(VerifyLayerUnLockUnfreezeTrueLayerUpdateDirtyResident), new[] { isFrozen, isLocked }));
        }

        [Test]
        public void VerifyLayerUnLockUnfreezeTrueLayerLockedFrozenUpdateAll()
        {
            const bool isFrozen = true;
            const bool isLocked = true;

            Assert.IsTrue(RunTest<bool>(nameof(VerifyLayerUnLockUnfreezeTrueLayerUpdateAllResident), new[] { isFrozen, isLocked }));
        }

        [Test]
        public void VerifyLayerUnLockUnfreezeTrueLayerFrozenUpdateAll()
        {
            const bool isFrozen = true;
            const bool isLocked = true;

            Assert.IsTrue(RunTest<bool>(nameof(VerifyLayerUnLockUnfreezeTrueLayerUpdateAllResident), new[] { isFrozen, isLocked }));
        }

        [Test]
        public void VerifyLayerUnLockUnfreezeTrueLayerLockedUpdateAll()
        {
            const bool isFrozen = true;
            const bool isLocked = true;

            Assert.IsTrue(RunTest<bool>(nameof(VerifyLayerUnLockUnfreezeTrueLayerUpdateAllResident), new[] { isFrozen, isLocked }));
        }

        [Test]
        public void VerifyLayerUnLockUnfreezeTrueLayerUpdateAll()
        {
            const bool isFrozen = true;
            const bool isLocked = true;

            Assert.IsTrue(RunTest<bool>(nameof(VerifyLayerUnLockUnfreezeTrueLayerUpdateAllResident), new[] { isFrozen, isLocked }));
        }



        public static bool VerifyLayerUnLockUnfreezeFalseLayerUpdateAllResident(bool[] data)
        {
            if (data.Length != 3) return false;

            var expectMethodRun =  data[0];
            var isFrozen = data[1];
            var isLocked = data[2];

            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acDb = acDoc.Database;
            var acEd = acDoc.Editor;

            TestDocumentStoreWithSettingsDisabled store;
            TestManagerForLayerSettings manager;

            using (var trans = acDb.TransactionManager.StartTransaction())
            {
                var ds = DataService.Current;
                ds.InvalidateStoreTypes();
                store = ds.GetStore<TestDocumentStoreWithSettingsDisabled>(Application.DocumentManager.MdiActiveDocument.Name);
                manager = store?.GetManager<TestManagerForLayerSettings>();

                trans.Commit();
            }
            
            if (manager == null || store.ShouldUnlockUnfreeze) return false;

            using (var trans = acDb.TransactionManager.StartTransaction())
            {
                var layer = acDb.GetLayer(TestDocumentStoreWithSettingsDisabled.LAYER_NAME);
                if (layer == null) return false;
                
                layer.UpgradeOpen();

                layer.IsFrozen = isFrozen;
                layer.IsLocked = isLocked;
                layer.IsOff = false;

                trans.Commit();
            }

            manager.DidUpdateAll = false;

            acEd.Command("_regen");
            
            if (manager.DidUpdateAll != expectMethodRun) return false;

            using (var trans = acDb.TransactionManager.StartTransaction())
            {
                var layer = acDb.GetLayer(TestDocumentStoreWithSettingsDisabled.LAYER_NAME);
                if (layer == null) return false;

                return layer.IsOff == false && layer.IsLocked == isLocked && layer.IsFrozen == isFrozen;
            }
        }

        public static bool VerifyLayerUnLockUnfreezeFalseLayerUpdateDirtyResident(bool[] data)
        {
            if (data.Length != 3) return false;

            var expectMethodRun = data[0];
            var isFrozen = data[1];
            var isLocked = data[2];

            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acDb = acDoc.Database;
            var acEd = acDoc.Editor;

            TestDocumentStoreWithSettingsDisabled store;
            TestManagerForLayerSettings manager;

            using (var trans = acDb.TransactionManager.StartTransaction())
            {
                var ds = DataService.Current;
                ds.InvalidateStoreTypes();
                store = ds.GetStore<TestDocumentStoreWithSettingsDisabled>(Application.DocumentManager.MdiActiveDocument.Name);
                manager = store?.GetManager<TestManagerForLayerSettings>();

                trans.Commit();
            }

            if (manager == null || store.ShouldUnlockUnfreeze) return false;

            using (var trans = acDb.TransactionManager.StartTransaction())
            {
                var layer = acDb.GetLayer(TestDocumentStoreWithSettingsDisabled.LAYER_NAME);
                if (layer == null) return false;

                layer.UpgradeOpen();

                layer.IsFrozen = isFrozen;
                layer.IsLocked = isLocked;
                layer.IsOff = false;

                trans.Commit();
            }

            manager.DidUpdateDirty = false;

            acEd.Command("_AECVERSION");

            if (manager.DidUpdateDirty != expectMethodRun) return false;

            using (var trans = acDb.TransactionManager.StartTransaction())
            {
                var layer = acDb.GetLayer(TestDocumentStoreWithSettingsDisabled.LAYER_NAME);
                if (layer == null) return false;

                return layer.IsOff == false && layer.IsLocked == isLocked && layer.IsFrozen == isFrozen;
            }
        }

        public static bool VerifyLayerUnLockUnfreezeTrueLayerUpdateAllResident(bool[] data)
        {
            if (data.Length != 2) return false;

            var isFrozen = data[0];
            var isLocked = data[1];

            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acDb = acDoc.Database;
            var acEd = acDoc.Editor;

            TestDocumentStoreWithSettingsEnabled store;
            TestManagerForLayerSettings manager;

            using (var trans = acDb.TransactionManager.StartTransaction())
            {
                var ds = DataService.Current;
                ds.InvalidateStoreTypes();
                store = ds.GetStore<TestDocumentStoreWithSettingsEnabled>(Application.DocumentManager.MdiActiveDocument.Name);
                manager = store?.GetManager<TestManagerForLayerSettings>();

                trans.Commit();
            }

            if (manager == null || !store.ShouldUnlockUnfreeze) return false;

            using (var trans = acDb.TransactionManager.StartTransaction())
            {
                var layer = acDb.GetLayer(TestDocumentStoreWithSettingsDisabled.LAYER_NAME);
                if (layer == null) return false;

                layer.UpgradeOpen();

                layer.IsFrozen = isFrozen;
                layer.IsLocked = isLocked;
                layer.IsOff = false;

                trans.Commit();
            }

            manager.DidUpdateAll = false;

            acEd.Command("_regen");

            if (!manager.DidUpdateAll) return false;

            using (var trans = acDb.TransactionManager.StartTransaction())
            {
                var layer = acDb.GetLayer(TestDocumentStoreWithSettingsDisabled.LAYER_NAME);
                if (layer == null) return false;

                return layer.IsOff == false && layer.IsLocked == isLocked && layer.IsFrozen == isFrozen;
            }
        }

        public static bool VerifyLayerUnLockUnfreezeTrueLayerUpdateDirtyResident(bool[] data)
        {
            if (data.Length != 2) return false;

            var isFrozen = data[0];
            var isLocked = data[1];

            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acDb = acDoc.Database;
            var acEd = acDoc.Editor;

            TestDocumentStoreWithSettingsEnabled store;
            TestManagerForLayerSettings manager;

            using (var trans = acDb.TransactionManager.StartTransaction())
            {
                var ds = DataService.Current;
                ds.InvalidateStoreTypes();
                store = ds.GetStore<TestDocumentStoreWithSettingsEnabled>(Application.DocumentManager.MdiActiveDocument.Name);
                manager = store?.GetManager<TestManagerForLayerSettings>();

                trans.Commit();
            }

            if (manager == null || !store.ShouldUnlockUnfreeze) return false;

            using (var trans = acDb.TransactionManager.StartTransaction())
            {
                var layer = acDb.GetLayer(TestDocumentStoreWithSettingsDisabled.LAYER_NAME);
                if (layer == null) return false;

                layer.UpgradeOpen();

                layer.IsFrozen = isFrozen;
                layer.IsLocked = isLocked;
                layer.IsOff = false;

                trans.Commit();
            }

            manager.DidUpdateDirty = false;

            acEd.Command("_AECVERSION");

            if (!manager.DidUpdateDirty) return false;

            using (var trans = acDb.TransactionManager.StartTransaction())
            {
                var layer = acDb.GetLayer(TestDocumentStoreWithSettingsDisabled.LAYER_NAME);
                if (layer == null) return false;

                return layer.IsOff == false && layer.IsLocked == isLocked && layer.IsFrozen == isFrozen;
            }
        }
    }
}
