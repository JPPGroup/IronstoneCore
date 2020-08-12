using Autodesk.AutoCAD.ApplicationServices.Core;
using Exception = System.Exception;

namespace Jpp.Ironstone.Core.ServiceInterfaces.Loggers
{
    public class ConsoleLogger : BaseLogger
    {
        private static void WriteEditorMessage(string message)
        {
            Application.DocumentManager.MdiActiveDocument?.Editor?.WriteMessage($"{message}\n");
        }

        public override void Entry(string message, Severity sev)
        {
            WriteEditorMessage($"{sev}:{message}");
        }

        public override void LogEvent(Event eventType, string eventParameters)
        {
            WriteEditorMessage($"{eventType}:{eventParameters}");
        }

        public override void LogException(Exception exception)
        {
            WriteEditorMessage($"ERROR:{exception}");
        }
    }
}
