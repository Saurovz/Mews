namespace Mews.Job.Scheduler.Workflows.ExecutorCleaner;

public sealed class ExecutorCleanerWorkflowConfiguration
{
    public TimeSpan Period { get; set; }

    public int RetentionPeriodDays { get; set; }
}
