using Mews.Atlas.Core.Tracing;
using Mews.Job.Scheduler.BuildingBlocks.Configuration.Authentication;
using Mews.Job.Scheduler.Core.Messaging.Jobs;
using Mews.Job.Scheduler.Domain.DateTimeProviders;
using Mews.Job.Scheduler.Domain.Executors;
using Mews.Job.Scheduler.Domain.JobExecutionLifecycle;
using Mews.Job.Scheduler.Domain.JobLifecycle;
using Mews.Job.Scheduler.Domain.Jobs;
using Mews.Job.Scheduler.Domain.JobScheduler;
using Mews.Job.Scheduler.Domain.Profiles;
using Mews.Job.Scheduler.Extensions;
using Mews.Job.Scheduler.Features.Authentication;
using Mews.Job.Scheduler.HostedServices;
using Mews.Job.Scheduler.Observability;
using Microsoft.Identity.Web;

namespace Mews.Job.Scheduler.Services;

public static class DomainServiceConfiguration
{
    /// <summary>
    /// Configures authentication for the application using Azure Active Directory (AzureAd).
    /// Automatically binds AzureAd configuration from appsettings.*.json and overrides the ClientId
    /// using the "S2S_APPLICATION_CLIENT_ID" environment variable for containerized deployments.
    /// Ensure the "AzureAd" section exists in all environment-specific appsettings files.
    /// </summary>
    /// <param name="services">The application's service collection to which authentication services are added.</param>
    /// <param name="configuration">The application configuration, which should include the "AzureAd" section.</param>
    /// <returns></returns>
    public static IServiceCollection AddAuthenticationService(this IServiceCollection services, IConfiguration configuration)
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("S2S_APPLICATION_CLIENT_ID")))
        {
            configuration["AzureAd:ClientId"] = Environment.GetEnvironmentVariable("S2S_APPLICATION_CLIENT_ID");
        }
        
        services
            .AddAuthentication()
            .AddMicrosoftIdentityWebApi(configuration);
        
        return services;
    }
    
    public static IServiceCollection AddAuthorizationService(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddConfiguration<AuthenticationTokensConfiguration>(configuration, AuthenticationTokensConfiguration.SectionName);
        services.AddScoped<AccessTokenAuthorizationFilter>();
        
        return services;
    }

    public static IServiceCollection AddSystemProfile(this IServiceCollection services)
    {
        services.AddSingleton<SystemProfile>();

        return services;
    }

    public static IServiceCollection AddDateTimeProvider(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        return services;
    }

    public static IServiceCollection AddHostedServiceConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        var hostedServiceConfigurations = services.AddConfiguration<HostedServiceConfigurations>(configuration, HostedServiceConfigurations.SectionName);
        foreach (var (serviceName, serviceConfiguration) in hostedServiceConfigurations.ConfigurationsByService)
        {
            services.AddKeyedSingleton(serviceName, serviceConfiguration);
        }

        return services;
    }

    public static IServiceCollection AddJobTimeoutHandlerService(this IServiceCollection services)
    {
        services.AddSingleton<JobTimeoutRetryCache>();
        services.AddSingleton<JobTimeoutHandlerMetrics>();
        services.AddScoped<JobTimeoutHandler>();

        return services;
    }

    public static IServiceCollection AddJobExecutionCleanerService(this IServiceCollection services)
    {
        services.AddSingleton<JobExecutionCleanerMetrics>();
        services.AddScoped<JobExecutionCleaner>();

        return services;
    }

    public static IServiceCollection AddJobCleanerService(this IServiceCollection services)
    {
        services.AddSingleton<JobCleanerMetrics>();
        services.AddScoped<JobCleaner>();

        return services;
    }

    public static IServiceCollection AddExecutorCleanerService(this IServiceCollection services)
    {
        services.AddSingleton<ExecutorCleanerMetrics>();
        services.AddScoped<ExecutorCleaner>();

        return services;
    }

    public static IServiceCollection AddJobSchedulerService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddConfiguration<JobPublishingConfiguration>(configuration, JobPublishingConfiguration.SectionName);

        services.AddSingleton<JobSchedulerMetrics>();
        services.AddSingleton<ICorrelationIdProvider, DefaultCorrelationIdProvider>();
        services.AddSingleton<IJobPublisher, JobPublisher>();
        services.AddScoped<JobScheduler>();

        services.AddHostedService<JobSchedulerService>();

        return services;
    }
}
