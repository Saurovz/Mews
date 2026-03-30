using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Mews.Job.Scheduler;

public static class LoggerExtensions
{
    public static void Info(this ILogger logger, string message, object? details = null)
    {
        logger.Detailed(message: message, details: details, level: LogLevel.Information);
    }

    public static void Detailed(this ILogger logger, string message, Exception? exception = null, object? details = null, LogLevel level = LogLevel.Information)
    {
        if (!logger.IsEnabled(level))
        {
            return;
        }

        var serializedDetails = JsonSerializer.Serialize(details);
        logger.Log(logLevel: level, exception: exception, "{Message:l} {Details:l}", message, serializedDetails);
    }
}
