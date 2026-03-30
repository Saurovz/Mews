using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TaxManager.Aspire.ServiceDefaults;

// Adds common .NET Aspire services: service discovery, resilience.
// This project should be referenced by each service project in your solution.
// See the documentation at https://mews.atlassian.net/wiki/spaces/AP/pages/351404133/Quickstart+Start+your+service+with+Aspire#The-Aspire-custom-service-defaults-project
public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        return builder;
    }
}
