namespace Mews.Job.Scheduler.Workflows.JobExecutionCleaner;

public class JobExecutionCleanerWorkflowConfiguration
{
    public TimeSpan Period { get; set; }

    public int RetentionPeriodDays { get; set; }
}
