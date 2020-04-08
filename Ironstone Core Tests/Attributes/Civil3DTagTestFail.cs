using System.Diagnostics;
using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using NUnit.Framework;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Jpp.Ironstone.Core.Tests.Attributes
{
    class Civil3DTagTestFail : IronstoneAutocadTestFixture
    {
        public Civil3DTagTestFail() : base(Assembly.GetExecutingAssembly(), typeof(Civil3DTagTestFail)) { }

        [Test]
        public void VerifyDrawingTag()
        {
            string dwgPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Test Drawings\\Civil3dTagged.dwg";
            
            Assert.IsTrue(RunTest<bool>(nameof(VerifyTag), dwgPath), "Event was not called");
        }
        
        public bool VerifyTag(string path)
        {
            bool eventFired = false;
            CoreExtensionApplication._current.Civil3DTagWarning +=
                delegate(object sender, Document args) { eventFired = true; };
            Application.DocumentManager.Open(path);

            return eventFired;
        }
    }
}
