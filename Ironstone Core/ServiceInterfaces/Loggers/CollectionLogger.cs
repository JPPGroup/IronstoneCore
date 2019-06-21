using System;
using System.Collections.Generic;
using System.Reflection;
using Autodesk.AutoCAD.Runtime;
using Exception = System.Exception;

namespace Jpp.Ironstone.Core.ServiceInterfaces.Loggers
{
    public class CollectionLogger : ILogger
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
        
        public void Entry(string message)
        {
            foreach (var logger in _loggers) logger.Entry(message);            
        }

        public void Entry(string message, Severity sev)
        {
            foreach (var logger in _loggers) logger.Entry(message, sev);
        }

        public void LogCommand(Type type, string method)
        {
            var rtMethod = type.GetRuntimeMethod(method, new Type[] { });
            var attribute = rtMethod.GetCustomAttribute<CommandMethodAttribute>();

            foreach (var logger in _loggers) logger.LogEvent(Event.Command, attribute.GlobalName);
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
