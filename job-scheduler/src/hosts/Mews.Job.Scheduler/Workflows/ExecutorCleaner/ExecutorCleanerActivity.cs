using JetBrains.Annotations;
using Mews.Atlas.Alerting;
using Microsoft.Extensions.Options;
using Temporalio.Activities;

namespace Mews.Job.Scheduler.Workflows.ExecutorCleaner;

[UsedImplicitly]
public class ExecutorCleanerActivity(
    Domain.JobExecutionLifecycle.ExecutorCleaner cleaner,
    IOptions<ExecutorCleanerWorkflowConfiguration> options,
    IIncidentReporter incidentReporter)
{
    private const string ActivityName = "ExecutorCleanup";

    [Activity(ActivityName)]
    public async Task CleanupAsync()
    {
        try
        {
            await cleaner.CleanAsync(options.Value.RetentionPeriodDays, ActivityExecutionContext.Current.CancellationToken);
        }
        catch (Exception exception)
        {
            incidentReporter.UnhandledException(exception, new
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
