using Microsoft.Extensions.Logging;
using System;

namespace DotNetSftp.UtilityClasses
{
    public class DiskLoggerProvider : ILoggerProvider
    {
        private string _logFilePath;

        public DiskLoggerProvider(string logFilePath)
        {
            _logFilePath = logFilePath;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ILogger CreateLogger(string categoryName)
        {
            ILogger diskLogger = new DiskLogger(_logFilePath);
            return diskLogger;
        }
    }
}