namespace Mews.Job.Scheduler.Domain.Jobs;

public sealed record ConfirmJobAfterExecutionParameters(bool IsExecutionSuccess, bool IsExecutionTimedOut, int TimeoutRetryCount, bool DeleteJob, string? FutureRunData = null);
