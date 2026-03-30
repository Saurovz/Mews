using Temporalio.Common;
using Temporalio.Workflows;

namespace Mews.Job.Scheduler.Workflows.JobTimeoutHandler;

[Workflow("JobTimeoutHandlerWorkflow")]
public class JobTimeoutHandlerWorkflow
{
    [WorkflowRun]
    public async Task RunAsync()
    {
        await Workflow.ExecuteActivityAsync<JobTimeoutHandlerActivity>(
            a => a.HandleTimeoutsAsync(),
            new ActivityOptions
            {
                StartToCloseTimeout = TimeSpan.FromMinutes(1),
                RetryPolicy = new RetryPolicy
                {
                    MaximumAttempts = 3
                }
            });
    }
}
