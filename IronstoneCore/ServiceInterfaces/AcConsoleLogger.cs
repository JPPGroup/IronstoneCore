using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Microsoft.Extensions.Logging;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{   

    public class AcConsoleLogger : ILogger
    {         
        public AcConsoleLogger()
        { }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;            

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string output = $"[IS {logLevel}] - {formatter(state, exception)}\n";
            Application.DocumentManager?.MdiActiveDocument?.Editor?.WriteMessage(output);           
        }
    }
}
