using Mews.Atlas.Temporal;
using Microsoft.Extensions.Options;
using Temporalio.Extensions.Hosting;

namespace Mews.Job.Scheduler.Workflows.JobCleaner;

public static class DependencyInjectionExtensions
{
    public static ITemporalWorkerServiceOptionsBuilder AddJobCleaner(this ITemporalWorkerServiceOptionsBuilder workerBuilder, IConfiguration configuration)
    {
        workerBuilder.Services.AddOptions<JobCleanerWorkflowConfiguration>()
            .Bind(configuration.GetSection("JobCleanerWorkflow"))
            .Validate(c => c.Period > TimeSpan.Zero)
            .Validate(c => c.RetentionPeriodDays > 0);

        workerBuilder
            .AddWorkflow<JobCleanerWorkflow>()
            .AddScopedActivities<JobCleanerActivity>()
            .AddSchedule(provider => JobCleanerSchedule.CreateDefinition(
                provider.GetRequiredService<IOptions<JobCleanerWorkflowConfiguration>>().Value.Period));
        return workerBuilder;
    }
}
