namespace Mews.Job.Scheduler.Workflows.JobCleaner;

public class JobCleanerWorkflowConfiguration
{
    public TimeSpan Period { get; set; }

    public int RetentionPeriodDays { get; set; }
}
