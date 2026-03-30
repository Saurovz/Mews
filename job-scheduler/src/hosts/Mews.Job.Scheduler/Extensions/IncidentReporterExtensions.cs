using Mews.Atlas.Alerting;
using Mews.Job.Scheduler.BuildingBlocks.Domain.PlatformTeams;

namespace Mews.Job.Scheduler;

public static class IncidentReporterExtensions
{
    public static void UnhandledException(this IIncidentReporter reporter, Exception exception, object? details = null)
    {
        UnhandledException(reporter, exception, PlatformTeams.Tooling, details);
    }
    
    public static void UnhandledException(this IIncidentReporter reporter, Exception exception, string team, object? details = null)
    {
        reporter.ReportUnhandledException(
            exception: exception,
            team: team,
            details: new
            {
                Exception = exception,
                ExceptionDetails = (exception as IDetailedException)?.DebugDetails,
                Details = details
            },
            incidentLevel: GetLevel(exception)
        );
    }

    private static IncidentLevel GetLevel(Exception? exception)
    {
        return exception switch
        {
            JobProcessingException processingException => GetLevel(processingException.InnerException),
            OperationTimeoutException => IncidentLevel.Warning,
            TransientPersistenceFaultException => IncidentLevel.Warning,
            ConcurrencyException => IncidentLevel.Warning,
            _ => IncidentLevel.Error
        };
    }
}
