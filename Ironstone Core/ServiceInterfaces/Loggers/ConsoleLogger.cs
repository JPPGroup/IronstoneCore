using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.EditorInput;

namespace Jpp.Ironstone.Core.ServiceInterfaces.Loggers
{
    class ConsoleLogger : ILogger
    {
        #region Interfaces
        public void Entry(string message)
        {
            Entry(message, Severity.Information);
        }

        public void Entry(string message, Severity sev)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument?.Editor;
            ed?.WriteMessage(message + "\n");
        }

        public void LogEvent(Event eventType, string eventParameters)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument?.Editor;
            ed?.WriteMessage(eventType + " - " + eventParameters + "\n");
        }
        #endregion
    }
}
