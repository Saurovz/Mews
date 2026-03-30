using Microsoft.AspNetCore.Mvc;

namespace Mews.Job.Scheduler.Extensions;

internal static class HttpResponseExtensions
{
    public static async Task WriteProblemAsJsonAsync(this HttpResponse response, ProblemDetails problemDetails, CancellationToken cancellationToken)
    {
        response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        response.ContentType = "application/problem+json";

        await response.WriteAsJsonAsync(problemDetails, cancellationToken);
    }
}
