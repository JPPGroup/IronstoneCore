using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jpp.Ironstone.Core.ServiceInterfaces.Loggers;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Jpp.Ironstone.Core.Tests.ServiceInterfaces.Loggers
{
    [TestFixture]
    class FileLoggerTests
    {
        [Test]
        public void LogFilePresent()
        {
            string logPath = Jpp.Ironstone.Core.Constants.APPDATA + "\\IronstoneLog.txt";

            if (File.Exists(logPath))
                File.Delete(logPath);

            using (FileLogger fl = new FileLogger())
            {
                fl.Entry("Test message");
            }

            if (File.Exists(logPath))
            {
                Assert.Pass("Log file found");
                File.Delete(Jpp.Ironstone.Core.Constants.APPDATA + "\\IronstoneLog.txt");
            }
            else
            {
                Assert.Fail("Log file not found");
            }
        }
    }
}
