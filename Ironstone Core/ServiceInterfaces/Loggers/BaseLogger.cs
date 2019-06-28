using Autodesk.AutoCAD.Runtime;
using System;
using System.Reflection;

namespace Jpp.Ironstone.Core.ServiceInterfaces.Loggers
{
    public abstract class BaseLogger : ILogger
    {
        public void Entry(string message)
        {
            Entry(message, Severity.Information);
        }

        public void LogCommand(Type type, string method)
        {
            var rtMethod = type.GetRuntimeMethod(method, new Type[] { });
            var attribute = rtMethod.GetCustomAttribute<CommandMethodAttribute>();

            LogEvent(Event.Command, attribute.GlobalName);
        }

        public abstract void Entry(string message, Severity sev);
        public abstract void LogEvent(Event eventType, string eventParameters);
        public abstract void LogException(System.Exception exception);
    }
}
