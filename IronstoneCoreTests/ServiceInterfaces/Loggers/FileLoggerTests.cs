using System;
using System.IO;
using Jpp.Ironstone.Core.Mocking;
using Jpp.Ironstone.Core.ServiceInterfaces.Loggers;
using NUnit.Framework;

namespace Jpp.Ironstone.Core.Tests.ServiceInterfaces.Loggers
{
    [TestFixture]
    internal class FileLoggerTests
    {
        [Test]
        public void LogFilePresent()
        {
            Configuration config = new Configuration();
            config.TestSettings();

            var logPath = config.LogFile;

            if (File.Exists(logPath)) File.Delete(logPath);

            using (var fl = new FileLogger(logPath))
            {
                fl.Entry("Test message");
            }

            if (File.Exists(logPath))
            {
                Assert.Pass("Log file found");
                File.Delete(logPath);
            }
            else
            {
                Assert.Fail("Log file not found");
            }
        }

        [Test]
        public void LogFileCanRead()
        {
            Configuration config = new Configuration();
            config.TestSettings();

            using (var log = new FileLogger(config.LogFile))
            {
                log.Entry("Test message");

                string contents;
                try
                {
                    using (TextReader tr = File.OpenText(config.LogFile))
                    {
                        contents = tr.ReadToEnd();
                    }
                }
                catch (Exception)
                {
                    contents = "";
                }

                Assert.False(string.IsNullOrEmpty(contents), "Should be able to read file contents.");
            }
        }
    }
}
