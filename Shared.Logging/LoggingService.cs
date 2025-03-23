using Microsoft.Extensions.Logging;

namespace Shared.Logging;

public class LoggerService<T> : ILoggerService<T>
{
    private readonly ILogger<T> logger;

    public LoggerService(ILogger<T> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger)); // Null check for logger
    }

    public void LogInformation(string message)
    {
        logger.LogInformation(message);
    }

    public void LogWarning(string message)
    {
        logger.LogWarning(message);
    }

    public void LogError(string message, Exception? exception = null)
    {
        if (exception != null)
        {
            logger.LogError(exception, message);
        }
        else
        {
            logger.LogError(message);
        }
    }

    public void LogDebug(string message)
    {
        logger.LogDebug(message);
    }

    public void LogCritical(string message, Exception? exception = null)
    {
        if (exception != null)
        {
            logger.LogCritical(exception, message);
        }
        else
        {
            logger.LogCritical(message);
        }
    }
}