using System;
using System.Collections.Generic;

namespace Jpp.Ironstone.Core.ServiceInterfaces.Loggers
{
    public class CollectionLogger : ILogger
    {
        private readonly ICollection<ILogger> _loggers = new List<ILogger>();

        public CollectionLogger()
        {
#if DEBUG
            _loggers.Add(new ConsoleLogger());
#else
            _loggers.Add(new TelemetryLogger());
            _loggers.Add(new CollectionLogger(new ConsoleLogger(), new FileLogger()));
#endif
        }

        public CollectionLogger(ILogger logger1, ILogger logger2)
        {
            _loggers.Add(logger1);
            _loggers.Add(logger2);
        }

        public void Entry(string message)
        {
            foreach (var logger in _loggers) logger.Entry(message);            
        }

        public void Entry(string message, Severity sev)
        {
            foreach (var logger in _loggers) logger.Entry(message, sev);
        }

        public void LogEvent(Event eventType, string eventParameters)
        {
            foreach (var logger in _loggers) logger.LogEvent(eventType, eventParameters);
        }

        public void LogException(Exception exception)
        {
            foreach (var logger in _loggers) logger.LogException(exception);
        }
    }
}
