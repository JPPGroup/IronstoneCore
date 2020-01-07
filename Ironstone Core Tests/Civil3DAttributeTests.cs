using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Runtime;
using Jpp.Ironstone.Core.Properties;
using NUnit.Framework;

namespace Jpp.Ironstone.Core.Tests
{
    [TestFixture]
    class Civil3DCommandAttributeTests : IronstoneTestFixture
    {
        public Civil3DCommandAttributeTests() : base(Assembly.GetExecutingAssembly(), typeof(Civil3DCommandAttributeTests)) { }

        [Test]
        public void Civil3DFailed()
        {
            bool result = RunTest<bool>(nameof(Civil3DFailedResident));

            Configuration config = new Configuration();
            config.TestSettings();
            string contents;

            using (TextReader tr = File.OpenText(config.LogFile))
            {
                contents = tr.ReadToEnd();
            }

            // TODO: Review referencing the resource string directly
            StringAssert.Contains("Command is not available unless running in Civil 3D", contents);
        }

        [IronstoneCommand]
        [Civil3D]
        [CommandMethod("Civil3DFailedResident")]
        public bool Civil3DFailedResident()
        {
            return true;
        }
    }
}
