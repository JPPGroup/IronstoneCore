using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace Jpp.Ironstone.Core.Tests.Attributes
{
    class Civil3DTagLoadCivil : IronstoneCivilTestFixture
    {
        public Civil3DTagLoadCivil() : base(Assembly.GetExecutingAssembly(), typeof(Civil3DTagLoadCivil), 
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Civil3dTagged.dwg") { }


        [Test]
        public void VerifyDrawingCanOpen()
        {
            Configuration config = new Configuration();
            config.TestSettings();
            string contents;

            using (TextReader tr = File.OpenText(config.LogFile))
            {
                contents = tr.ReadToEnd();
            }

            Assert.IsTrue(!contents.Contains("Civil3D features will not function in this drawing. Proceed at own risk"));
        }
    }
}
