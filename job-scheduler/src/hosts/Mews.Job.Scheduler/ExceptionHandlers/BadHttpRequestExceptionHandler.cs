using System.Diagnostics;
using Mews.Atlas.Alerting;
using Mews.Atlas.OpenTelemetry;
using Mews.Job.Scheduler.BuildingBlocks.Domain.PlatformTeams;
using Mews.Job.Scheduler.Extensions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Mews.Job.Scheduler.ExceptionHandlers;

internal sealed class BadHttpRequestExceptionHandler(IIncidentReporter incidentReporter) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not BadHttpRequestException)
        {
            return false;
        }

        Activity.Current?.RecordExceptionWithStatus(exception);
        
        ProblemDetails problemDetails;
        var details = await httpContext.GetApiExceptionDetails(StatusCodes.Status400BadRequest, cancellationToken);

        // Temporary handling of the use-case, so that it does not create incidents for every occurence of request body timing out
        // due to data arriving too slowly. This should be removed once the issue is resolved.
        if (exception.Message.Contains("Reading the request body timed out due to data arriving too slowly.", StringComparison.InvariantCulture))
        {
            incidentReporter.Report(
                exception.ToString(),
                PlatformTeams.Tooling,
                IncidentLevel.Warning,
                exception,
                details
            );

            // Returning as a 500 error so that the client can retry the request. The use-case fixes after an immediate retry.
            problemDetails = new ProblemDetails
            {
                Title = JobSchedulerServiceExceptionReason.InternalError.ToString(),
                Detail = "An unexpected error occurred.",
                Status = StatusCodes.Status500InternalServerError
            };

            await httpContext.Response.WriteProblemAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        problemDetails = new ProblemDetails
        {
            Title = nameof(BadHttpRequestException),
            Detail = exception.Message,
            Status = StatusCodes.Status400BadRequest
        };

        await httpContext.Response.WriteProblemAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
