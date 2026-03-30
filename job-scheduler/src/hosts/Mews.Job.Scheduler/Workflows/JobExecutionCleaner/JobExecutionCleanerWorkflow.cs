using Temporalio.Common;
using Temporalio.Workflows;

namespace Mews.Job.Scheduler.Workflows.JobExecutionCleaner;

[Workflow("JobExecutionCleanerWorkflow")]
public class JobExecutionCleanerWorkflow
{
    [WorkflowRun]
    public async Task RunAsync()
    {
        await Workflow.ExecuteActivityAsync<JobExecutionCleanerActivity>(
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
