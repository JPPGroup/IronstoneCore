using System;
using System.Linq;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices.Core;
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
        public void VerifyTestManagerSaveLoadManagedObject()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyTestManagerSaveLoadManagedObjectResident)));
        }

        public bool VerifyTestManagerSaveLoadManagedObjectResident()
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
}
