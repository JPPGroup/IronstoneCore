using System;
using System.IO;
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
            var logPath = CoreExtensionApplication._current.Configuration.LogFile;

            if (File.Exists(logPath)) File.Delete(logPath);

            using (var fl = new FileLogger())
            {
                fl.Entry("Test message");
            }

            if (File.Exists(logPath))
            {
                Assert.Pass("Log file found");
                File.Delete(CoreExtensionApplication._current.Configuration.LogFile);
            }
            else
            {
                Assert.Fail("Log file not found");
            }
        }

        [Test]
        public void LogFileCanRead()
        {
            using (var log = new FileLogger())
            {
                log.Entry("Test message");

                string contents;
                try
                {
                    using (TextReader tr = File.OpenText(CoreExtensionApplication._current.Configuration.LogFile))
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
