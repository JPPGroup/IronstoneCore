using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;

namespace Jpp.Ironstone.Core.Tests.TestObjects
{
    public class TestManagerForLayerSettings : AbstractDrawingObjectManager<TestDrawingObject>
    {
        public bool DidUpdateAll { get; set; }
        public bool DidUpdateDirty { get; set; }

        public TestManagerForLayerSettings(Document document, ILogger log) : base(document, log) { }
        private TestManagerForLayerSettings() { }
        

        public override void UpdateDirty()
        {
            DidUpdateDirty = true;
        }

        public override void UpdateAll()
        {
            DidUpdateAll = true;
        }
    }
}
