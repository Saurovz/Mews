using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mews.Atlas.Alerting;
using Mews.Atlas.OpenTelemetry;
using Mews.Job.Scheduler.Extensions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

[assembly: InternalsVisibleTo("Mews.Job.Scheduler.UnitTests")]

namespace Mews.Job.Scheduler.ExceptionHandlers;

internal sealed class JobSchedulerServiceExceptionHandler(IIncidentReporter incidentReporter) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not JobSchedulerServiceException schedulerServiceException)
        {
            return false;
        }
        
        Activity.Current?.RecordExceptionWithStatus(exception);

        var statusCode = GetStatusCode(schedulerServiceException);
        if (schedulerServiceException.Reason == JobSchedulerServiceExceptionReason.InternalError)
        {
            var details = await httpContext.GetApiExceptionDetails(statusCode, cancellationToken);
            incidentReporter.UnhandledException(exception, details);
        }
        
        var problemDetails = new ProblemDetails
        {
            Title = schedulerServiceException.Reason.ToString(),
            Detail = schedulerServiceException.Message,
            Status = statusCode
        };

        await httpContext.Response.WriteProblemAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private int GetStatusCode(JobSchedulerServiceException schedulerServiceException)
    {
        return schedulerServiceException.Reason switch
        {
            JobSchedulerServiceExceptionReason.EntityNotFound => 404,
            JobSchedulerServiceExceptionReason.EntityInvalidStateTransition => 400,
            JobSchedulerServiceExceptionReason.ServiceCommunicationProblem => 503,
            JobSchedulerServiceExceptionReason.EntityProcessingConflict => 409,
            _ => 500
        };
    }
}
