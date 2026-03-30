using System.Globalization;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using Mews.Job.Scheduler.BuildingBlocks.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Mews.Job.Scheduler.Core.EntityFrameworkCore.Helpers;

public static class SqlActionExceptionMapper
{
    // The exception messages are copied from real exceptions that we catch (not which we create and throw).
    // If there is a weird formatting or typo, it's most likely intentional, please don't fix it.
    private static readonly string SqlExceptionSevereErrorMessage = "A severe error occurred on the current command.  The results, if any, should be discarded.\r\nA severe error occurred on the current command.  The results, if any, should be discarded.";
    private static readonly string SqlExceptionSevereErrorOperationCanceledByUserMessage = "A severe error occurred on the current command.  The results, if any, should be discarded.\r\nOperation cancelled by user.";
    private static readonly string SqlExceptionConversionOverflowsMessage = "Conversion overflows.";
    private static readonly string SqlExceptionIoExceptionReadConnectionResetMessage = "Unable to read data from the transport connection: Connection reset by peer.";
    private static readonly string SqlExceptionIoExceptionWriteConnectionResetMessage = "Unable to write data to the transport connection: Connection reset by peer.";
    private static readonly string SqlExceptionIoExceptionUnexpectedEoFMessage = "Received an unexpected EOF or 0 bytes from the transport stream.";

    public static ExceptionDispatchInfo Map(ExceptionDispatchInfo ex)
    {
        var exception = ex.SourceException;

        // Explicitly short-circuit to pass-through case to simplify other conditions.
        if (exception is OperationTimeoutException or OperationCanceledException)
        {
            return ex;
        }

        // Wrap recognized timeout causes to OperationTimeoutException for proper timeout handling.
        if (exception.IsOrContainsTimeoutException())
        {
            return MapNestedTimeoutException(exception);
        }

        if (exception is DbUpdateConcurrencyException updateException)
        {
            return MapDbUpdateConcurrencyException(updateException);
        }

        var innerException = exception.InnerException;
        var innerInnerException = innerException?.InnerException;
        var sqlException = new[] { innerInnerException, innerException, exception }.OfType<SqlException>().FirstOrDefault();

        if (sqlException is not null)
        {
            var number = sqlException.Number;

            // Catches several unique constraint violations.
            // 2601 is unique index, 2627 more generic constraint violation.
            if (number == 2601 || number == 2627)
            {
                return MapSqlException2601And2627(sqlException);
            }

            // 4002 catches "The incoming tabular data stream (TDS) protocol stream is incorrect. The stream ended unexpectedly."
            // This is usually just an underlying connection interruption.
            // 8009 catches a variant of "The incoming tabular data stream (TDS) remote procedure call (RPC) protocol stream is incorrect."
            if (number == 4002 || number == 8009)
            {
                return MapToTransientException(exception);
            }

            // Catches all sorts of underlying network problems.
            // Can include timeout, but that should be handled in an earlier check.
            if (sqlException.InnerException is System.ComponentModel.Win32Exception)
            {
                return MapToTransientException(exception);
            }

            // Usually caused by short term network issues, restart of the server and a lingering connection left in the pool.
            if (sqlException.InnerException is IOException ioException &&
                (ioException.Message == SqlExceptionIoExceptionReadConnectionResetMessage ||
                 ioException.Message == SqlExceptionIoExceptionWriteConnectionResetMessage ||
                 ioException.Message == SqlExceptionIoExceptionUnexpectedEoFMessage))
            {
                return MapToTransientException(exception);
            }

            // Catches "A severe error occurred on the current command. The results, if any, should be discarded."
            // This can be a variety of errors from schema mismatch to just network interrupts. No way to tell at this level.
            if (sqlException.InnerException is null && sqlException.Message == SqlExceptionSevereErrorMessage)
            {
                return MapToTransientException(exception);
            }

            if (sqlException.InnerException is null && sqlException.Message == SqlExceptionSevereErrorOperationCanceledByUserMessage)
            {
                return MapToTransientException(exception);
            }
        }
        else if (innerInnerException is not null)
        {
            var message = innerInnerException.ToMessageString(CultureInfo.InvariantCulture);
            if (message == SqlExceptionConversionOverflowsMessage ||
                message.StartsWith("Parameter value '", StringComparison.InvariantCulture) && message.EndsWith("' is out of range.", StringComparison.InvariantCulture))
            {
                // When EF or System.Data refuse to process the value because it's larger than the underlying SQL type.
                return ExceptionDispatchInfo.Capture(
                    new InvalidValueException(innerException: innerInnerException)
                );
            }
        }

        return ex;
    }

    private static ExceptionDispatchInfo MapDbUpdateConcurrencyException(DbUpdateConcurrencyException exception)
    {
        var affectedEntities = exception.Entries.Select(e => new
        {
            EntityType = e.Entity.GetType().Name,
            EntityName = e.Entity.ToString(),
            EntityId = (e.Entity as IEntity<Guid>)?.Id
        });

        var concurrencyException = new ConcurrencyException(exception, new
        {
            AffectedEntities = affectedEntities
        });

        return ExceptionDispatchInfo.Capture(concurrencyException);
    }

    private static ExceptionDispatchInfo MapNestedTimeoutException(Exception exception)
    {
        return ExceptionDispatchInfo.Capture(new OperationTimeoutException(exception));
    }

    private static ExceptionDispatchInfo MapToTransientException(Exception exception)
    {
        return ExceptionDispatchInfo.Capture(new TransientPersistenceFaultException(exception));
    }

    private static ExceptionDispatchInfo MapSqlException2601And2627(SqlException sqlException)
    {
        // Try to get the property name from the error message (e.g. "... unique index 'IX_User_Name'").
        var propertyName = "Id";
        var message = sqlException.ToMessageString(CultureInfo.InvariantCulture);
        var match = Regex.Match(message, "unique index '[^']+_([^_']+)'");
        if (match.Success)
        {
            propertyName = match.Groups[1].Value;
        }

        return ExceptionDispatchInfo.Capture(new UniquenessException(propertyName, sqlException));
    }
}
