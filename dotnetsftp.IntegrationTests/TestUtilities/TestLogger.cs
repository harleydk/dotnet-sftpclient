using System;
using Microsoft.Extensions.Logging;

namespace dotnetsftp.IntegrationTests.TestUtilities
{
    public class TestLogger : ILogger
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            System.Diagnostics.Debug.WriteLine($"{state.ToString()}");
            return;
        }
        

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}