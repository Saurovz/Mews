namespace Mews.Job.Scheduler.Domain.Jobs;

[Flags]
public enum JobOptions
{
    None = 0,
    ParallelExecution = 1 << 0,
    TimeoutRetryDisabled = 1 << 1,
    IsFatal = 1 << 2,
    TimeoutAsWarning = 1 << 3
}
