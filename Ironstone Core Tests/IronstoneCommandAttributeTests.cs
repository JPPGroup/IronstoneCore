using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.Runtime;
using NUnit.Framework;

namespace Jpp.Ironstone.Core.Tests
{
    [TestFixture]
    class IronstoneCommandAttributeTests : IronstoneTestFixture
    {
        public IronstoneCommandAttributeTests() : base(Assembly.GetExecutingAssembly(), typeof(IronstoneCommandAttributeTests)) { }

        [TestCase(nameof(LoggerCalled_WithAttribute_WithCommandResident), ExpectedResult = true)]
        [TestCase(nameof(LoggerCalled_WithAttribute_WithoutCommand_Resident), ExpectedResult = false)]
        [TestCase(nameof(LoggerCalled_WithoutAttribute_WithoutCommand_Resident), ExpectedResult = false)]
        [TestCase(nameof(LoggerCalled_WithoutAttribute_WithCommand_Resident), ExpectedResult = false)]
        public bool LoggerCalled(string commandName)
        {
            bool result = RunTest<bool>(commandName);

            Configuration config = new Configuration();
            config.TestSettings();
            string contents;

            using (TextReader tr = File.OpenText(config.LogFile))
            {
                contents = tr.ReadToEnd();
            }

            return result && contents.Contains(commandName);
        }

        [IronstoneCommand]
        [CommandMethod("LoggerCalled_WithAttribute_WithCommandResident")]
        public bool LoggerCalled_WithAttribute_WithCommandResident()
        {
            return true;
        }

        [IronstoneCommand]
        public bool LoggerCalled_WithAttribute_WithoutCommand_Resident()
        {
            return true;
        }

        [CommandMethod("LoggerCalled_WithoutAttribute_WithCommand_Resident")]
        public bool LoggerCalled_WithoutAttribute_WithCommand_Resident()
        {
            return true;
        }

        public bool LoggerCalled_WithoutAttribute_WithoutCommand_Resident()
        {
            return true;
        }
    }
}
