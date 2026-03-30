using Mews.Atlas.Temporal;
using Mews.Job.Scheduler.Workflows.JobCleaner;
using Microsoft.Extensions.Options;
using Temporalio.Extensions.Hosting;

namespace Mews.Job.Scheduler.Workflows.ExecutorCleaner;

public static class DependencyInjectionExtensions
{
    public static ITemporalWorkerServiceOptionsBuilder AddExecutorCleaner(this ITemporalWorkerServiceOptionsBuilder workerBuilder, IConfiguration configuration)
    {
        workerBuilder.Services.AddOptions<ExecutorCleanerWorkflowConfiguration>()
            .Bind(configuration.GetSection("ExecutorCleanerWorkflow"))
            .Validate(c => c.Period > TimeSpan.Zero)
            .Validate(c => c.RetentionPeriodDays > 0);

        workerBuilder
            .AddWorkflow<ExecutorCleanerWorkflow>()
            .AddScopedActivities<ExecutorCleanerActivity>()
            .AddSchedule(provider => ExecutorCleanerSchedule.CreateDefinition(
                provider.GetRequiredService<IOptions<ExecutorCleanerWorkflowConfiguration>>().Value.Period)
            );

        return workerBuilder;
    }
}
