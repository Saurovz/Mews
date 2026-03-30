using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;

namespace Mews.Job.Scheduler.Core.EntityFrameworkCore.Helpers;

public static class ExceptionExtensions
{
    /// <summary>
    /// Returns true if this exception (or ANY inner exceptions) is a timeout exception..
    /// </summary>
    /// <remarks>Looks at inner exceptions transitively and even if the parent exception is not an AggregateException.</remarks>
    public static bool IsOrContainsTimeoutException(this Exception e)
    {
        return e.GetAllExceptionsTransitive().Any(ex => ex.IsTimeoutException());
    }

    public static string ToMessageString(this Exception e, CultureInfo culture)
    {
        var originalCulture = Thread.CurrentThread.CurrentUICulture;

        Thread.CurrentThread.CurrentUICulture = culture;
        var message = e.Message;
        Thread.CurrentThread.CurrentUICulture = originalCulture;

        return message;
    }

    /// <summary>
    /// Returns a transitive collection of all the inner exceptions. Skips AggregateExceptions and instead only returns the ones contained inside AggregateExceptions.
    /// </summary>
    private static IEnumerable<Exception> GetAllExceptionsTransitive(this Exception exception)
    {
        if (exception is AggregateException aggregateException)
        {
            var transitiveInnerExceptions = aggregateException.InnerExceptions.SelectMany(e => GetAllExceptionsTransitive(e));
            foreach (var innerException in transitiveInnerExceptions)
            {
                yield return innerException;
            }
        }
        else
        {
            yield return exception;

            if (exception.InnerException is not null)
            {
                foreach (var innerException in GetAllExceptionsTransitive(exception.InnerException))
                {
                    yield return innerException;
                }
            }
        }
    }

    private static bool IsTimeoutException(this Exception e)
    {
        var messageString = e.ToMessageString(CultureInfo.InvariantCulture);
        var message = string.IsNullOrEmpty(messageString) ? messageString : "";

        return e switch
        {
            WebException web => message.In("The operation has timed out", "Unable to connect to the remote server"),
            SocketException socket => (
                message.Contains("connected party did not properly respond after a period of time", StringComparison.InvariantCulture) ||
                message.Contains("Connection timed out", StringComparison.InvariantCulture)
            ),
            Win32Exception win => message.In("The wait operation timed out", "The semaphore timeout period has expired"),
            SqlException sql => (
                message.ToLowerInvariant().Contains("timeout expired.", StringComparison.InvariantCulture) ||
                message.Contains("Lock request time out period exceeded.", StringComparison.InvariantCulture) ||
                message.Contains("not currently available", StringComparison.InvariantCulture) ||
                message.Contains("ran out of internal resources", StringComparison.InvariantCulture) ||
                message.Contains("Server provided routing information, but timeout already expired.", StringComparison.InvariantCulture) ||
                message.Contains("Operation cancelled by user.", StringComparison.InvariantCulture)
            ),
            TimeoutException timeout => true,
            InvalidOperationException i => (
                message.StartsWith("Timeout expired.  The timeout period elapsed prior to obtaining a connection from the pool.", StringComparison.InvariantCulture) ||
                message.Contains("Operation cancelled by user.", StringComparison.InvariantCulture)
            ),
            RetryLimitExceededException r => message.StartsWith("Maximum number of retries", StringComparison.InvariantCulture),
            _ => false
        };
    }

    private static bool In<T>(this T value, params T[]? values)
    {
        return (values ?? Array.Empty<T>()).Any(v => v!.Equals(value));
    }
}
