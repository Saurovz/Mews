using FluentValidation;
using Mews.Atlas.AspNetCore;
using Mews.Atlas.Azure.Identity;
using Mews.Atlas.Messaging.ServiceBus;
using Mews.Atlas.Temporal;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Mews.Job.Scheduler.BuildingBlocks.Infrastructure;
using Mews.Job.Scheduler.Common.Logging;
using Mews.Job.Scheduler.Core.Application;
using Mews.Job.Scheduler.Environments;
using Mews.Job.Scheduler.ExceptionHandlers;
using Mews.Job.Scheduler.Extensions;
using Mews.Job.Scheduler.Services;
using Mews.Job.Scheduler.Services.HealthChecks;
using Mews.Job.Scheduler.Workflows.ExecutorCleaner;
using Mews.Job.Scheduler.Workflows.JobCleaner;
using Mews.Job.Scheduler.Workflows.JobExecutionCleaner;
using Mews.Job.Scheduler.Workflows.JobTimeoutHandler;
using Serilog;

namespace Mews.Job.Scheduler;

public sealed class Program
{
    public static async Task<int> Main(string[] args)
    {
        Logs.Configure();

        try
        {
            Log.Information("JobScheduler - Starting web host.");

            var builder = CreateWebApplicationBuilder(args);
            var app = CreateWebApplication(builder);
            await app.RunAsync();

            return 0;
        }
        catch (HostAbortedException ex)
        {
            Log.Fatal(ex, "JobScheduler - Host aborted!");
            throw;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "JobScheduler - Host terminated unexpectedly!");
            throw;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    public static WebApplication CreateWebApplication(WebApplicationBuilder builder)
    {
        var app = builder.Build();
        ConfigureApp(app, builder.Environment.EnvironmentName);
        return app;
    }

    public static WebApplicationBuilder CreateWebApplicationBuilder(string[] args, string? environment = default)
    {
        var builder = WebApplication.CreateBuilder(args);
        environment ??= builder.Environment.EnvironmentName;
        builder.Configuration.CheckEnvironmentConfiguration(environment);

        ConfigureHost(builder);
        ConfigureServices(builder, environment);

        return builder;
    }

    private static void ConfigureHost(WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog(logger: Log.Logger, dispose: true);
    }

    private static void ConfigureServices(WebApplicationBuilder builder,  string environment)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        // Persistence
        services.AddDbContext(builder, configuration, environment);
        services.AddDbContextProvider();
        services.AddPersistence();

        // Atlas
        services.AddKeyVault(configuration, environment);
        services.AddFeatureFlags(configuration, environment);
        services.AddIncidentReporter(configuration, environment);
        services.AddAzureServiceBusIntegration(configuration, [typeof(ApiModule)], () => new TokenCredentialFactory().Create(SupportedEnvironments.IsLocalEnvironment(environment)));
        services.AddSecurityHeaders();
        
        // Auth 
        services.AddAuthenticationService(configuration);
        services.AddAuthorizationService(configuration);
        
        // Domain Services
        services.AddSystemProfile();
        services.AddDateTimeProvider();
        services.AddHostedServiceConfigurations(configuration);
        services.AddJobTimeoutHandlerService();
        services.AddJobExecutionCleanerService();
        services.AddJobCleanerService();
        services.AddExecutorCleanerService();
        services.AddJobSchedulerService(configuration);

        // Other Services
        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger: Log.Logger, dispose: true));

        AddTemporalWorker(services, configuration);

        services.AddOpenTelemetryServices();
        services.AddControllers();
        services.AddCustomCors();
        services.AddJsonConfiguration();
        services.AddSwagger();

        services.AddExceptionHandler<JobSchedulerServiceExceptionHandler>();
        services.AddExceptionHandler<BadHttpRequestExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<ApiModule>());
        services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<ApplicationModule>());
        services.AddValidatorsFromAssemblyContaining(typeof(ApiModule));

        var healthChecksBuilder = services.AddHealthChecks().AddCheck<ApplicationHealthCheck>(nameof(ApplicationHealthCheck));
        healthChecksBuilder.AddCheck<DatabaseHealthCheck>(nameof(DatabaseHealthCheck), tags: new[] { "startup" });
    }

    private static void AddTemporalWorker(IServiceCollection services, IConfiguration configuration)
    {
        var temporalConnectionConfiguration = services.TryAddConfiguration<TemporalConnectionConfiguration>(
            configuration, "Temporal");
        if (temporalConnectionConfiguration is null)
        {
            return;
        }

        services.AddTemporalClient()
            .PostConfigure<IServiceProvider>((o, serviceProvider) =>
                temporalConnectionConfiguration.ConfigureClientConnectionOptions(o, serviceProvider));

        services.AddMewsHostedTemporalWorker(temporalConnectionConfiguration.TaskQueue)
            .AddExecutorCleaner(configuration)
            .AddJobCleaner(configuration)
            .AddJobExecutionCleaner(configuration)
            .AddJobTimeoutHandler(configuration);
    }

    private static void ConfigureApp(WebApplication app, string environment)
    {
        var applicationLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
        applicationLifetime.ApplicationStarted.Register(() =>
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var maxCompletionPortThreads);
            logger.LogInformation("Max worker threads: {MaxWorkerThreads}, max completion port threads: {MaxCompletionPortThreads}", maxWorkerThreads, maxCompletionPortThreads);
        });

        if (SupportedEnvironments.IsLiveEnvironment(environment))
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseExceptionHandler(_ => { });
        app.UseCors(ApiModuleConstants.CorsPolicy);
        if (SupportedEnvironments.IsDeveloperEnvironment(environment) || SupportedEnvironments.IsAspireEnvironment(environment))
        {
            app.UseOpenApi();
            app.UseSwaggerUi();
        }
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseAtlasSecurityHeaders();
        app.MapControllers();
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = healthCheck => !healthCheck.Tags.Contains("startup"),
            ResponseWriter = HealthCheckExtensions.WriteResponse
        });
        app.MapHealthChecks("/startup", new HealthCheckOptions
        {
            Predicate = healthCheck => healthCheck.Tags.Contains("startup")
        });
    }
}
