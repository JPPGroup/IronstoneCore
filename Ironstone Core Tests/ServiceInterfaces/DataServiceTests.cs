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
        //TODO: Figure out why this wont work
        public void StoreCreation()
        {
            //Application.UserConfigurationManager.OpenCurrentProfile().
            //Application.DocumentManager.AppContextNewDocument("C:\\Users\\Michael\\AppData\\Local\\Autodesk\\C3D 2019\\eng\\Template\\acad.dwt");
            /*Application.DocumentManager.AppContextOpenDocument("C:\\Users\\Michael\\Documents\\Drawing1.dwg");
            string doc = Application.DocumentManager.CurrentDocument.Name;*/
            
            Assert.Pass("Test needs to be re-worked");
        }

        [Test, Category("Integration")]
        public void GetCurrentDocumentStoreOnEmptyDoc()
        {
            //Gets the default document store, but on an empty document ensure creation works
            var result = DataService.Current.GetStore<DocumentStore>(Application.DocumentManager.CurrentDocument.Name);
            Assert.IsNotNull(result);
        }
    }
}
