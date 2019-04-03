using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Jpp.Ironstone.Core.ServiceInterfaces.Loggers
{
    class FileLogger : ILogger, IDisposable
    {
        private Process Process => Process.GetCurrentProcess();
        private string filePath;
        private Logger _logger;

        public FileLogger()
        {
            filePath = CoreExtensionApplication._current.Configuration.LogFile;

            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = filePath, KeepFileOpen = false, ArchiveAboveSize = 1000000, MaxArchiveFiles = 10};
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);
            NLog.LogManager.Configuration = config;
            _logger = NLog.LogManager.GetCurrentClassLogger();
        }

        public void Entry(string message)
        {
            _logger.Info($"{Process.Id} : {message}");
        }

        public void Entry(string message, Severity sev)
        {
            message = $"{Process.Id} : {message}";
            switch (sev)
            {
                    case Severity.Debug:
                        _logger.Debug(message);
                    break;

                case Severity.Information:
                    _logger.Info(message);
                    break;

                case Severity.Warning:
                    _logger.Warn(message);
                    break;

                case Severity.Error:
                    _logger.Error(message);
                    break;

                case Severity.Crash:
                    _logger.Fatal(message);
                    break;
            }
        }

        public void LogEvent(Event eventType, string eventParameters)
        {
            _logger.Trace($"{Process.Id} : {eventType} - {eventParameters}");
        }

        public void LogException(Exception exception)
        {
            _logger.Error(exception, Process.Id.ToString);
        }

        public void Dispose()
        {
            NLog.LogManager.Flush();
        }

        ~FileLogger()
        {
            NLog.LogManager.Flush();
        }
    }
}
