using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace Jpp.Ironstone.Core.Tests.Attributes
{
    class Civil3DTagLoadCivil : IronstoneCivilTestFixture
    {
        public Civil3DTagLoadCivil() : base(Assembly.GetExecutingAssembly(), typeof(Civil3DTagLoadCivil), 
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Test Drawings\\Civil3dTagged.dwg") { }


        [Test]
        public void VerifyDrawingCanOpen()
        {
            string contents;
            
            using (TextReader tr = LogHelper.GetLogReader())
            {
                contents = tr.ReadToEnd();
            }

            Assert.IsTrue(!contents.Contains("Civil3D features will not function in this drawing. Proceed at own risk"));
        }
    }
}
