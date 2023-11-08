using Jpp.Ironstone.Core.Mocking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.IO;
using System.Reflection;

namespace Jpp.Ironstone.Core.Tests.ServiceInterfaces
{
    [TestFixture]
    public class LoggerTests : IronstoneTestFixture
    {
        public LoggerTests() : base(Assembly.GetExecutingAssembly(), typeof(LoggerTests)) { }

        public Configuration Config;
        private string _loggerThumbprint = "2658e8d0-6a84-4b04-a4c9-2e2336149498";

        public override void Setup()
        {
            Config = new Configuration();
            Config.TestSettings();
            ConfigurationHelper.CreateConfiguration(Config);

            //Clear existing log before loading
            if (File.Exists(Config.LogFile))
                File.Delete(Config.LogFile);
        }

        [Test, Order(1)]
        public void VerifyTraceLogging()
        {
            Assert.True(RunTest<bool>(nameof(VerifyTraceLoggingResident)));

            using (TextReader tr = LogHelper.GetLogReader())
            {
                string contents = tr.ReadToEnd();
                if (!contents.Contains("2658e8d0-6a84-4b04-a4c9-2e2336149498"))
                {
                    Assert.Fail("Thumbprint not found in logs.");
                }
            }
        }

        public bool VerifyTraceLoggingResident()
        {
            var logger = CoreExtensionApplication._current.Container.GetRequiredService<ILogger<LoggerTests>>();
            logger.LogTrace(_loggerThumbprint);
            return true;
        }

        [Test, Order(2)]
        public void VerifyRunningLog()
        {
            Assert.IsTrue(File.Exists(LogHelper.GetLogName()));
        }

        [Test, Order(2)]
        public void VerifyCentralLog()
        {
            Assert.IsTrue(File.Exists(LogHelper.GetAppDataLogName()));
        }
    }
}
