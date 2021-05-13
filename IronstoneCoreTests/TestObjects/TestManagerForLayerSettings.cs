using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Jpp.Ironstone.Core.Tests.TestObjects
{
    public class TestManagerForLayerSettings : AbstractDrawingObjectManager<TestDrawingObject>
    {
        public bool DidUpdateAll { get; set; }
        public bool DidUpdateDirty { get; set; }

        public TestManagerForLayerSettings(Document document, ILogger<CoreExtensionApplication> log, IConfiguration settings) : base(document, log, settings) { }
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
