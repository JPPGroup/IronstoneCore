using System;
using System.Diagnostics;
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
        public void VerifyLayerCreation()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyLayerCreationResident)));
        }

        public bool VerifyLayerCreationResident()
        {
            using (Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                DataService ds = DataService.Current;
                ds.InvalidateStoreTypes();
                var store = ds.GetStore<TestDocumentStore>(Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager
                    .MdiActiveDocument.Name);

                trans.Commit();
            }

            using (Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager
                .StartTransaction())
            {
                return Application.DocumentManager.MdiActiveDocument.Database
                           .GetLayer("TestDocumentStoreLayer") !=
                       null;
            }
        }
    }
}
