using JetBrains.Annotations;
using Mews.Atlas.Alerting;
using Temporalio.Activities;

namespace Mews.Job.Scheduler.Workflows.JobTimeoutHandler;

[UsedImplicitly]
public class JobTimeoutHandlerActivity(
    Domain.JobLifecycle.JobTimeoutHandler timeoutHandler,
    IIncidentReporter incidentReporter)
{
    private const string ActivityName = "JobTimeoutHandler";

    [Activity(ActivityName)]
    public async Task HandleTimeoutsAsync()
    {
        try
        {
            await timeoutHandler.FindAndMarkAsTimedOut(
                ActivityExecutionContext.Current.CancellationToken);
        }
        catch (ConcurrencyException)
        {
            // Someone else has already handled the timeout.
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
