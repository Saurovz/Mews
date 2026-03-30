using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

namespace TaxManager.Extensions;

public static class HealthCheckExtensions
{
    public static Task WriteResponse(
        HttpContext context,
        HealthReport report)
    {
        if (report.Status is HealthStatus.Unhealthy or HealthStatus.Degraded)
        {
            Log.Error("Health check failed: {HealthReport}", report.Entries.Select(e =>
                new
                {
                    Key = e.Key,
                    Status = e.Value.Status,
                    Duration = e.Value.Duration,
                    Error = e.Value.Exception?.Message
                }));
        }
        
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(
            new
            {
                Status = report.Status.ToString(),
                Duration = report.TotalDuration,
                Info = report.Entries
                    .Select(e =>
                        new
                        {
                            Key = e.Key,
                            Description = e.Value.Description,
                            Duration = e.Value.Duration,
                            Status = Enum.GetName(
                                typeof(HealthStatus),
                                e.Value.Status),
                            Error = e.Value.Exception?.Message,
                            Data = e.Value.Data
                        })
                    .ToList()
            },
            jsonSerializerOptions);

        context.Response.ContentType = MediaTypeNames.Application.Json;
        return context.Response.WriteAsync(json);
    }
}
