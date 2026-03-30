namespace Mews.Job.Scheduler.Domain.JobExecutions;

public sealed record JobExecutionCreateParameters(Guid JobId, string ExecutorTypeNameValue, DateTime StartUtc, string TransactionIdentifier, JobExecutionState State = JobExecutionState.InProgress);
