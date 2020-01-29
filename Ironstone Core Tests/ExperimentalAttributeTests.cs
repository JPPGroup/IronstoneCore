using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.Runtime;
using NUnit.Framework;

namespace Jpp.Ironstone.Core.Tests
{
    [TestFixture]
    class ExperimentalAttributeTests : IronstoneTestFixture
    {
        public ExperimentalAttributeTests() : base(Assembly.GetExecutingAssembly(), typeof(ExperimentalAttributeTests)) { }

        

        [TestCase(nameof(ExperimentalRanResident), false)]
        [TestCase(nameof(ExperimentalFailedResident), true)]
        [TestCase(nameof(ExperimentalFailedNoSettingResident), true)]
        public void ExperimentalCalled(string commandName, bool expectedToBlock)
        {
            Configuration config = new Configuration();
            config.TestSettings();
            
            /*if(File.Exists(config.LogFile))
                File.Delete(config.LogFile);*/

            bool result = RunTest<bool>(commandName);

            string contents;

            using (TextReader tr = File.OpenText(config.LogFile))
            {
                contents = tr.ReadToEnd();
            }

            //return contents.Contains("Command is experimental and not currently enabled.");
            Assert.Multiple(() =>
            {
                // TODO: Review referencing the resource string directly
                if (expectedToBlock)
                {
                    Assert.IsFalse(result, "Method should not run");
                    StringAssert.Contains("Command is experimental and not currently enabled.", contents);
                }
                else
                {
                    Assert.IsTrue(result, "Method should run");
                }
            });
        }

        [Experimental]
        [CommandMethod("ExperimentalRanResident")]
        public bool ExperimentalRanResident()
        {
            return true;
        }

        /// <summary>
        /// Tests for command being blocked when experimental setting is set to false
        /// </summary>
        /// <returns></returns>
        [Experimental]
        [CommandMethod("ExperimentalFailedResident")]
        public bool ExperimentalFailedResident()
        {
            return true;
        }

        /// <summary>
        /// Tests for command being blocked when experimental setting is not set
        /// </summary>
        /// <returns></returns>
        
        [CommandMethod("ExperimentalFailedNoSettingResident")]
        [Experimental]
        public bool ExperimentalFailedNoSettingResident()
        {
            return true;
        }

    }
}
