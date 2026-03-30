using System.Diagnostics;
using Mews.Atlas.Alerting;
using Mews.Atlas.OpenTelemetry;
using Mews.Job.Scheduler.Extensions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Mews.Job.Scheduler.ExceptionHandlers;

internal sealed class GlobalExceptionHandler(IIncidentReporter incidentReporter) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        Activity.Current?.RecordExceptionWithStatus(exception);
        
        var details = await httpContext.GetApiExceptionDetails(httpContext.Response.StatusCode, cancellationToken);
        incidentReporter.UnhandledException(exception, details);

        var problemDetails = new ProblemDetails
        {
            Title = JobSchedulerServiceExceptionReason.InternalError.ToString(),
            Detail = "An unexpected error occurred.",
            Status = httpContext.Response.StatusCode
        };

        await httpContext.Response.WriteProblemAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
