using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace Jpp.Ironstone.Core.Tests.Attributes
{
    class Civil3DTagLoadCoreConsole : IronstoneTestFixture
    {
        public Civil3DTagLoadCoreConsole() : base(Assembly.GetExecutingAssembly(), typeof(Civil3DTagLoadCoreConsole), 
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Test Drawings\\Civil3dTagged.dwg") { }


        [Test]
        public void VerifyDrawingCantOpen()
        {
            Configuration config = new Configuration();
            config.TestSettings();
            string contents;

            using (TextReader tr = File.OpenText(config.LogFile))
            {
                contents = tr.ReadToEnd();
            }

            Assert.IsTrue(contents.Contains("Civil3D features will not function in this drawing. Proceed at own risk"));
        }

        [Test]
        public void VerifyLibraryNotPresent()
        {
            bool exists = File.Exists(Path.Combine(Assembly.GetExecutingAssembly().Location, "AeccDbMgd.dll"));

            Assert.IsTrue(!exists);
        }
    }
}
