using JetBrains.Annotations;
using Mews.Atlas.Alerting;
using Microsoft.Extensions.Options;
using Temporalio.Activities;

namespace Mews.Job.Scheduler.Workflows.JobCleaner;

[UsedImplicitly]
public class JobCleanerActivity(
    Domain.JobExecutionLifecycle.JobCleaner cleaner,
    IOptions<JobCleanerWorkflowConfiguration> options,
    IIncidentReporter incidentReporter)
{
    private const string ActivityName = "JobCleanup";

    [Activity(ActivityName)]
    public async Task CleanupAsync()
    {
        try
        {
            await cleaner.CleanAsync(
                options.Value.RetentionPeriodDays,
                ActivityExecutionContext.Current.CancellationToken);
        }
        catch (Exception e)
        {
            incidentReporter.UnhandledException(
                e,
                new
                {
                    ActivityName,
                    ActivityExecutionContext.Current.Info.ActivityId,
                    ActivityExecutionContext.Current.Info.WorkflowId
                });
            // Rethrow to fail the activity and trigger workflow retry policy.
            throw;
        }
    }
}
