using Temporalio.Common;
using Temporalio.Workflows;

namespace Mews.Job.Scheduler.Workflows.JobCleaner;

[Workflow("JobCleanerWorkflow")]
public class JobCleanerWorkflow
{
    [WorkflowRun]
    public async Task RunAsync()
    {
        await Workflow.ExecuteActivityAsync<JobCleanerActivity>(
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
