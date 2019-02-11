using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using NUnit.Framework;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Jpp.Ironstone.Core.Tests.ServiceInterfaces
{
    [TestFixture]
    class DataServiceTests
    {
        [Test]
        public void VerifyStoreTypesLoaded()
        {
            DataService.Current.PopulateStoreTypes();
            Assert.AreEqual(DataService.Current._storesList.Count, 2);
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
            bool app = Application.DocumentManager.IsApplicationContext;
            Application.DocumentManager.AppContextNewDocument("acad.dwt");
            //Application.DocumentManager.CurrentDocument.CloseAndDiscard();
        }*/

        /*[Test, Category("Integration")]
        public void GetCurrentDocumentStoreOnEmptyDoc()
        {
            //Gets the default document store, but on an empty document ensure creation works
            var result = DataService.Current.GetStore<DocumentStore>(Application.DocumentManager.CurrentDocument.Name);
            Assert.IsNotNull(result);
        }*/
        }
    }
