using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace Jpp.Ironstone.Core.Tests.Attributes
{
    class Civil3DTagLoadAutocad : IronstoneAutocadTestFixture
    {
        public Civil3DTagLoadAutocad() : base(Assembly.GetExecutingAssembly(), typeof(Civil3DTagLoadAutocad), 
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Test Drawings\\Civil3dTagged.dwg") { }


        [Test]
        public void VerifyDrawingCantOpen()
        {
            string contents;
            
            using (TextReader tr = LogHelper.GetLogReader())
            {
                contents = tr.ReadToEnd();
            }

            Assert.IsTrue(contents.Contains("Civil3D features will not function"));
        }

        [Test]
        public void VerifyLibraryNotPresent()
        {
            bool exists = File.Exists(Path.Combine(Assembly.GetExecutingAssembly().Location, "AeccDbMgd.dll"));

            Assert.IsTrue(!exists);
        }
    }
}
