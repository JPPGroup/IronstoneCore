using System;
using System.Diagnostics;
using NLog;
using Exception = System.Exception;

namespace Jpp.Ironstone.Core.ServiceInterfaces.Loggers
{
    public class FileLogger : BaseLogger, IDisposable
    {
        private static Process Process => Process.GetCurrentProcess();
        private readonly Logger _logger;

        public FileLogger(string path)
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget
            {
                Name= "logfile",
                FileName = path,
                KeepFileOpen = false,
                ArchiveAboveSize = 1000000,
                MaxArchiveFiles = 10
            };

            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);

            LogManager.Configuration = config;

            _logger = LogManager.GetCurrentClassLogger();
        }

        public void Dispose()
        {
            LogManager.Flush();
        }

        public override void Entry(string message, Severity sev)
        {
            message = $"{Process.Id}:{message}";
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

        public override void LogEvent(Event eventType, string eventParameters)
        {
            _logger.Trace($"{Process.Id}:{eventType}:{eventParameters}");
        }

        public override void LogException(Exception exception)
        {
            _logger.Error(exception, Process.Id.ToString);
        }

        ~FileLogger()
        {
            LogManager.Flush();
        }
    }
}
