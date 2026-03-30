namespace Mews.Job.Scheduler.Domain.JobExecutions;

public enum JobExecutionState
{
    InProgress = 0,
    Success = 1,
    Warning = 2,
    Error = 3,
    Timeout = 4
}

[Flags]
public enum JobExecutionStates
{
    InProgress = 1 << 0,
    Success = 1 << 1,
    Warning = 1 << 2,
    Error = 1 << 3,
    Timeout = 1 << 4
}
