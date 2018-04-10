using System;
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace CakeExtracter.Logging.Loggers
{
    class EnterpriseLibraryLogger : ILogger
    {
        private void Write(LogEntry logEntry)
        {
            Microsoft.Practices.EnterpriseLibrary.Logging.Logger.Write(logEntry);
        }

        private LogEntry LogEntry(TraceEventType severity, string format, params object[] args)
        {
            return new LogEntry
            {
                Message = string.Format(format, args),
                Severity = severity
            };
        }

        public void Info(string format, params object[] args)
        {
            Write(LogEntry(TraceEventType.Information, format, args));
        }

        public void Warn(string format, params object[] args)
        {
            Write(LogEntry(TraceEventType.Warning, format, args));
        }

        public void Error(Exception exception)
        {
            Write(LogEntry(TraceEventType.Error, "{0}", exception.Message));
        }

        public void Trace(string format, params object[] args)
        {
            Write(LogEntry(TraceEventType.Verbose, format, args));
        }
    }
}