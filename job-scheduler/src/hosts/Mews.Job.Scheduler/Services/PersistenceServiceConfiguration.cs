using Mews.Job.Scheduler.BuildingBlocks.Infrastructure;
using Mews.Job.Scheduler.Configuration;
using Mews.Job.Scheduler.Core.EntityFrameworkCore;
using Mews.Job.Scheduler.Core.EntityFrameworkCore.Executors;
using Mews.Job.Scheduler.Core.EntityFrameworkCore.Helpers;
using Mews.Job.Scheduler.Core.EntityFrameworkCore.JobExecutions;
using Mews.Job.Scheduler.Core.EntityFrameworkCore.JobPersistence;
using Mews.Job.Scheduler.Core.EntityFrameworkCore.Jobs;
using Mews.Job.Scheduler.Domain;
using Mews.Job.Scheduler.Domain.Executors;
using Mews.Job.Scheduler.Domain.JobExecutions;
using Mews.Job.Scheduler.Domain.Jobs;
using Mews.Job.Scheduler.Environments;
using Mews.Job.Scheduler.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Mews.Job.Scheduler.Services;

public static class PersistenceServiceConfiguration
{
    public static IServiceCollection AddDbContext(this IServiceCollection services, WebApplicationBuilder builder,  IConfiguration configuration, string environment)
    {
        var sqlConfiguration = services.AddConfiguration<SqlConfiguration>(configuration, SqlConfiguration.SectionName);
        var retryExecutionRetryStrategy = sqlConfiguration.RetryExecutionStrategy;
        services.AddDbContext<JobSchedulerDbContext>(options => options.UseSqlServer(
            configuration.GetConnectionString(ApiModuleConstants.ConnectionStringName),
            sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: retryExecutionRetryStrategy.MaxRetryCount,
                    maxRetryDelay: retryExecutionRetryStrategy.MaxRetryDelay,
                    errorNumbersToAdd: SqlTransientExceptionRetryHelper.GetAdditionalSqlErrorNumbersToRetry()
                );

                // Workaround for https://github.com/dotnet/aspire/issues/1023
                if (SupportedEnvironments.IsAspireEnvironment(environment))
                {
                    sqlOptions.ExecutionStrategy(esd => new RetryingSqlServerRetryingExecutionStrategy(esd));
                }
            }
        ));

        if(SupportedEnvironments.IsAspireEnvironment(environment))
        {
            // Disable Aspire default retries as we're using a custom execution strategy
            builder.EnrichSqlServerDbContext<JobSchedulerDbContext>(settings =>
            {
                settings.DisableRetry = true;
            });
        }

        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IJobExecutionRepository, JobExecutionRepository>();
        services.AddScoped<IExecutorRepository, ExecutorRepository>();
        services.AddScoped<IJobPersistence, JobPersistence>();

        return services;
    }
}
