using Mews.Atlas.Temporal;
using Microsoft.Extensions.Options;
using Temporalio.Extensions.Hosting;

namespace Mews.Job.Scheduler.Workflows.JobExecutionCleaner;

public static class DependencyInjectionExtensions
{
    public static ITemporalWorkerServiceOptionsBuilder AddJobExecutionCleaner(this ITemporalWorkerServiceOptionsBuilder workerBuilder, IConfiguration configuration)
    {
        workerBuilder.Services.AddOptions<JobExecutionCleanerWorkflowConfiguration>()
            .Bind(configuration.GetSection("JobExecutionCleanerWorkflow"))
            .Validate(c => c.Period > TimeSpan.Zero)
            .Validate(c => c.RetentionPeriodDays > 0);

        workerBuilder
            .AddWorkflow<JobExecutionCleanerWorkflow>()
            .AddScopedActivities<JobExecutionCleanerActivity>()
            .AddSchedule(provider => JobExecutionCleanerSchedule.CreateDefinition(
                provider.GetRequiredService<IOptions<JobExecutionCleanerWorkflowConfiguration>>().Value.Period));
        return workerBuilder;
    }
}
