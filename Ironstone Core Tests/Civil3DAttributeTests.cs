using System.IO;
using System.Reflection;
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
        
        public bool Civil3DFailedResident()
        {
            bool ran = false;
            Civil3DFailedResidentInner(ref ran);
            return ran;
        }

        [Civil3D]
        public void Civil3DFailedResidentInner(ref bool ran)
        {
            ran = true;
        }
    }
}
