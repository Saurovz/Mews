namespace Mews.Job.Scheduler.Core.EntityFrameworkCore.Helpers;

public sealed class SqlTransientExceptionRetryHelper
{
    /// <summary>
    /// Returns additional SQL error numbers to retry on, in addition to those provided in the EF Core transient exception detector.
    /// See <see href="https://github.com/dotnet/efcore/blob/main/src/EFCore.SqlServer/Storage/Internal/SqlServerTransientExceptionDetector.cs">EF Core Transient Exception Detector</see> for more details.
    /// </summary>
    /// <returns>
    /// An enumerable of additional SQL error numbers which should trigger a retry when encountered.
    /// </returns>
    public static IEnumerable<int> GetAdditionalSqlErrorNumbersToRetry()
    {
        return
        [
            31, // A connection was successfully established with the server, but then an error occurred during the pre-login handshake. (provider: SSL Provider, error: 31 - Encryption(ssl/tls) handshake failed)
            35 // A transport-level error has occurred when receiving results from the server. (provider: TCP Provider, error: 35 - An internal exception was caught).
        ];
    }
}
