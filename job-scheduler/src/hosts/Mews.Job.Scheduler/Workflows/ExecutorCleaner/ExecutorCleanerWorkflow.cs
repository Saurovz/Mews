using Temporalio.Common;
using Temporalio.Workflows;

namespace Mews.Job.Scheduler.Workflows.ExecutorCleaner;

[Workflow("ExecutorCleanerWorkflow")]
public sealed class ExecutorCleanerWorkflow
{
    [WorkflowRun]
    public async Task RunAsync()
    {
        await Workflow.ExecuteActivityAsync<ExecutorCleanerActivity>(
            a => a.CleanupAsync(),
            new ActivityOptions
            {
                StartToCloseTimeout = TimeSpan.FromMinutes(15),
                RetryPolicy = new RetryPolicy
                {
                    MaximumAttempts = 3
                }
            });
    }
}
