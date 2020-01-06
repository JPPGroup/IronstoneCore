using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.Runtime;
using NUnit.Framework;

namespace Jpp.Ironstone.Core.Tests
{
    [TestFixture]
    // TODO: Add a test for whne civil 3d is running to check this works ok. Need Civil 3d test environment to implement 
    class IronstoneCommandAttributeTests : IronstoneTestFixture
    {
        public IronstoneCommandAttributeTests() : base(Assembly.GetExecutingAssembly(), typeof(IronstoneCommandAttributeTests)) { }

        [Test]
        public void LoggerCalled()
        {
            bool result = RunTest<bool>(nameof(LoggerCalledResident));

            Configuration config = new Configuration();
            config.TestSettings();
            string contents;

            using (TextReader tr = File.OpenText(config.LogFile))
            {
                contents = tr.ReadToEnd();
            }

            StringAssert.Contains("LoggerCalledResident", contents);
        }

        [IronstoneCommand]
        [CommandMethod("LoggerCalledResident")]
        public bool LoggerCalledResident()
        {
            return true;
        }
    }
}
