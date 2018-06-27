using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace DotNetSftp.UtilityClasses
{
    /// <summary>
    /// A simple disk-logger.
    /// </summary>
    /// <remarks>Responds to all log-levels.</remarks>
    public class DiskLogger : ILogger
    {
        private readonly string _logLocation;

        public DiskLogger(string logLocation)
        {
            _logLocation = logLocation;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            // The BeginScope gives the logger an opportunity to know about disposal for cleanup,
            // but for this specific application it won't be needed.
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            var message = formatter(state, exception);

            if (string.IsNullOrEmpty(message))
                return;

            message = $"{ logLevel }: {message}";

            if (exception != null)
                message += Environment.NewLine + Environment.NewLine + exception.ToString();

            File.AppendAllText(_logLocation, message);
        }
    }
}