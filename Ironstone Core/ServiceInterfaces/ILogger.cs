using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    /// <summary>
    /// Generic definition of a logging service
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Add message to log with default severity of Information
        /// </summary>
        /// <param name="message">Message to be added</param>
        void Entry(string message);

        /// <summary>
        /// Add message to log with specified severity
        /// </summary>
        /// <param name="message">Message to be added</param>
        /// <param name="sev">Severity of message</param>
        void Entry(string message, Severity sev);

        void LogEvent(Event eventType, string eventParameters);
    }

    /// <summary>
    /// Indicates the severity of the log message
    /// </summary>
    public enum Severity
    {
        Debug,
        Information,
        Warning,
        Error,
        Crash
    }

    public enum Event
    {
        Command,
    }
}
