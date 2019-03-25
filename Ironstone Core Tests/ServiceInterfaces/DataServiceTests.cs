using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.AcTestFramework;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.Mocking;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Core.Tests.TestObjects;
using NUnit.Framework;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Jpp.Ironstone.Core.Tests.ServiceInterfaces
{
    [TestFixture]
    class DataServiceTests : BaseNUnitTestFixture
    {
        public DataServiceTests() : base(Assembly.GetExecutingAssembly(), typeof(DataServiceTests), "IronstoneCore.dll") { }

        public override void Setup()
        {
            Configuration config = new Configuration();
            config.TestSettings();
            ConfigurationHelper.CreateConfiguration(config);
        }

       [Test]
        public void VerifyStoreTypesLoaded()
        {
            int count = RunTest<int>("VerifyStoreTypesLoadedResident");
            Assert.AreEqual(2, count);
        }

        public int VerifyStoreTypesLoadedResident()
        {
            DataService ds = DataService.Current;
            ds.PopulateStoreTypes();
            return DataService.Current._storesList.Count;
        }

        [Test]
        public void VerifyTestStoreLoaded()
        {
            Assert.True(RunTest<bool>("VerifyTestStoreLoadedResident"));
        }

        public bool VerifyTestStoreLoadedResident()
        {
            try
            {
                DataService ds = DataService.Current;
                ds.InvalidateStoreTypes();
                var store = ds.GetStore<TestDocumentStore>(Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager
                    .MdiActiveDocument.Name);
                if (store != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }
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
    }
}
