using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.EditorInput;

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
