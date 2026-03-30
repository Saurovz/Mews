namespace Mews.Job.Scheduler.Domain.Jobs;

public enum JobState
{
    Pending = 0,
    InProgress = 1,
    Executed = 2,
    Scheduled = 3
}

[Flags]
public enum JobStates
{
    Pending = 1 << 0,
    InProgress = 1 << 1,
    Executed = 1 << 2,
    Scheduled = 1 << 3
}
