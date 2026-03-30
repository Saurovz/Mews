using System.Reflection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TaxManager.Services.HealthChecks;

public class AssemblyVersionHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthCheckResult.Healthy($"Assembly version: {Assembly.GetEntryAssembly()?.GetName().Version}"));
    }
}
