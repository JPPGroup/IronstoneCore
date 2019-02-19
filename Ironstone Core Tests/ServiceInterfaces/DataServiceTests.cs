using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using BaseTestLibrary;
using BaseTestLibrary.Serialization;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using NUnit.Framework;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Jpp.Ironstone.Core.Tests.ServiceInterfaces
{
    [TestFixture]
    class DataServiceTests : BaseTest
    {
        [Test]
        public void VerifyStoreTypesLoaded()
        {
            TestResponse resp = RunTest("VerifyStoreTypesLoadedResident", null);
            Assert.True(resp.Result);
            int? count = resp.Data as int?;
            Assert.NotNull(count);

            Assert.AreEqual(count, 2);
        }

        public int VerifyStoreTypesLoadedResident(object data)
        {
            DataService.Current.PopulateStoreTypes();
            return DataService.Current._storesList.Count;
        }

        
        /*[Test]
        public void StoreCreationOnDocumentCreation()
        {
            //TODO: Why does this end with an excpetion??
            Application.DocumentManager.AppContextNewDocument("acad.dwt");
            string doc = Application.DocumentManager.CurrentDocument.Name;

            Assert.Pass("Test needs to be re-worked");
        }*/

        /*[Test]
        public void StoreRemovaleOnDocumentDestruction()
        {
            var result = CoreExtensionApplication._current.SyncContext.BeginInvoke(new TestDelegate(() =>
                    {

                        string doc = Application.DocumentManager.CurrentDocument.Name;
                        string doc2 = Application.DocumentManager.MdiActiveDocument.Name;
                        bool app = Application.DocumentManager.IsApplicationContext;
                        //Application.DocumentManager.AppContextNewDocument("acad.dwt");
                        Application.DocumentManager.AppContextOpenDocument(
                            "C:\\Users\\michaell\\Documents\\Centreline.dwg");
                        doc = Application.DocumentManager.CurrentDocument.Name;
                        doc2 = Application.DocumentManager.MdiActiveDocument.Name;
                        Application.DocumentManager.CurrentDocument.CloseAndDiscard();

                        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Test");

                        Assert.Fail("Designed to fail");
                    }
                ));

            result.AsyncWaitHandle.WaitOne();

            Console.WriteLine("Test");

        }

        /*[Test, Category("Integration")]
        public void GetCurrentDocumentStoreOnEmptyDoc()
        {
            //Gets the default document store, but on an empty document ensure creation works
            var result = DataService.Current.GetStore<DocumentStore>(Application.DocumentManager.CurrentDocument.Name);
            Assert.IsNotNull(result);
        }*/
        public override Guid FixtureGuid { get; } = new Guid();

        public override string DrawingFile { get; } =
            @"C:\Repos\Ironstone\ironstone-core\Ironstone Core Tests\Blank.dwg";
        public override string AssemblyPath { get; } = Assembly.GetExecutingAssembly().Location;
        public override string AssemblyType { get; } = typeof(DataServiceTests).FullName;
    }

    delegate void TestDelegate();
}
