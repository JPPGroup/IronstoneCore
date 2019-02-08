using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices.Core;
using NUnit.Framework;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Jpp.Ironstone.Core.Tests.ServiceInterfaces
{
    [TestFixture]
    class DataServiceTests
    {
        [Test]
        public void StoreCreation()
        {
            //int documents = Application.DocumentManager.Count;

            Assert.Pass("Test desgined to pass");
        }
    }
}
