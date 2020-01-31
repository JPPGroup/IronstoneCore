using System.Diagnostics;
using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using NUnit.Framework;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Jpp.Ironstone.Core.Tests.Attributes
{
    class Civil3DTagTestPass : IronstoneCivilTestFixture
    {
        public Civil3DTagTestPass() : base(Assembly.GetExecutingAssembly(), typeof(Civil3DTagTestPass)) { }

        [Test]
        public void VerifyDrawingTag()
        {
            Assert.IsTrue(RunTest<bool>(nameof(VerifyTagAdded)));
        }
        
        public bool VerifyTagAdded()
        {
            if (CoreExtensionApplication._current.CheckDrawingForCivil3D(Application.DocumentManager.MdiActiveDocument))
                throw new InvalidDataException("Drawing already tagged");
            
            TagDrawing();

            return CoreExtensionApplication._current.CheckDrawingForCivil3D(Application.DocumentManager
                .MdiActiveDocument);
        }

        [Civil3D]
        public void TagDrawing()
        {
            int i;
        }

        [Test]
        public void VerifyDrawingCanOpen()
        {
            string dwgPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Civil3dTagged.dwg";

            Assert.IsTrue(!RunTest<bool>(nameof(VerifyTagEvent), dwgPath), "Event was called");
        }

        public bool VerifyTagEvent(string path)
        {
            bool eventFired = false;
            CoreExtensionApplication._current.Civil3DTagWarning +=
                delegate (object sender, Document args) { eventFired = true; };
            Application.DocumentManager.Open(path);

            return eventFired;
        }
    }
}
