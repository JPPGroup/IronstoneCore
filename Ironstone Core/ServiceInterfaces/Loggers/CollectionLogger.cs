using System.Collections.Generic;
using Exception = System.Exception;

namespace Jpp.Ironstone.Core.ServiceInterfaces.Loggers
{
    public class CollectionLogger : BaseLogger
    {
        private readonly ICollection<ILogger> _loggers = new List<ILogger>();

        public CollectionLogger()
        {

#if !DEBUG
            _loggers.Add(new TelemetryLogger());
#endif
            _loggers.Add(new ConsoleLogger());
            _loggers.Add(new FileLogger(CoreExtensionApplication._current.Configuration.LogFile));
        }

        public override void Entry(string message, Severity sev)
        {
            foreach (var logger in _loggers) logger.Entry(message, sev);
        }

        public override void LogEvent(Event eventType, string eventParameters)
        {
            foreach (var logger in _loggers) logger.LogEvent(eventType, eventParameters);
        }

        public override void LogException(Exception exception)
        {
            foreach (var logger in _loggers) logger.LogException(exception);
        }
    }
}
