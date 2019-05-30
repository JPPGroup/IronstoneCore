using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.Core.Tests.TestObjects
{
    public class TestDrawingObjectManager : AbstractDrawingObjectManager<TestDrawingObject>
    {
        public TestDrawingObjectManager(Document document) : base(document) { }
        private TestDrawingObjectManager() { }

    }
}
