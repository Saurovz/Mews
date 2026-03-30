using System.Reflection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Mews.Job.Scheduler.Services.HealthChecks;

public sealed class ApplicationHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthCheckResult.Healthy($"Assembly version: {Assembly.GetEntryAssembly()?.GetName().Version}"));
    }
}
