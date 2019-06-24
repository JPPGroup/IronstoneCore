using System;

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

        /// <summary>
        /// Add message to log when a command is invoked
        /// </summary>
        /// <param name="type">Type for command method</param>
        /// <param name="method">Method name of command</param>
        void LogCommand(Type type, string method);

        void LogEvent(Event eventType, string eventParameters);
        void LogException(Exception exception);
    }

    /// <summary>
    /// Indicates the severity of the log message
    /// </summary>
    public enum Severity
    {
        Debug = 0,
        Information = 100,
        Warning = 200,
        Error = 300,
        Crash = 999
    }

    public enum Event
    {
        Command,
        Message
    }
}
