using Microsoft.Extensions.Logging;

namespace UserManagement.Tests
{
    public class TestLogger<T> : ILogger<T>
    {
        public List<string> LogMessages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);
            LogMessages.Add(message);
        }
        
        IDisposable? ILogger.BeginScope<TState>(TState state)
        {
            BeginScope(state);
            return null;
        }
    }
}