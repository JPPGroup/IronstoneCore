using System;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Exception = System.Exception;

namespace Jpp.Ironstone.Core.ServiceInterfaces.Loggers
{
    public class ConsoleLogger : ILogger
    {
        #region Interfaces
        public void Entry(string message)
        {
            Entry(message, Severity.Information);
        }

        public void Entry(string message, Severity sev)
        {
            WriteMessage(message);            
        }

        public void LogCommand(Type type, string method)
        {
            var rtMethod = type.GetRuntimeMethod(method, new Type[] { });
            var attribute = rtMethod.GetCustomAttribute<CommandMethodAttribute>();

            LogEvent(Event.Command, attribute.GlobalName);
        }

        public void LogEvent(Event eventType, string eventParameters)
        {
            WriteMessage($"{eventType} - {eventParameters}");
        }

        public void LogException(Exception exception)
        {
            WriteMessage(exception.ToString());
        }
        #endregion

        private static void WriteMessage(string message)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument?.Editor;
            ed?.WriteMessage($"{message}\n");
        }
    }
}
