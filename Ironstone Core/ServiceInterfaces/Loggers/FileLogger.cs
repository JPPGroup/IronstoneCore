using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jpp.Ironstone.Core.ServiceInterfaces.Loggers
{
    class FileLogger : ILogger
    {
        private string filePath;

        public FileLogger()
        {
            filePath = Assembly.GetExecutingAssembly().Location + "\\IronstoneLog.txt";
        }

        public void Entry(string message)
        {
            AppendText($"{Severity.Information} - {message}");
        }

        public void Entry(string message, Severity sev)
        {
            AppendText($"{Severity.Information} - {message}");
        }

        public void LogEvent(Event eventType, string eventParameters)
        {
            AppendText($"{eventType} - {eventParameters}");
        }

        public void LogException(Exception exception)
        {
            AppendText(exception.Message);
        }

        private void AppendText(string message)
        {
            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.WriteLine($"{DateTime.Now} - {message}");
            }

        }
    }
}
