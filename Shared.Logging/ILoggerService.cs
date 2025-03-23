namespace Shared.Logging;

public interface ILoggerService<T>
{
    void LogInformation(string message);
    void LogWarning(string message);
    void LogError(string message, Exception? exception = null);
    void LogDebug(string message);
    void LogCritical(string message, Exception? exception = null);
}