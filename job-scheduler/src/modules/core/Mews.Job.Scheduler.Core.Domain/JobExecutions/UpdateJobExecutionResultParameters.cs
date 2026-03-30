namespace Mews.Job.Scheduler.Domain.JobExecutions;

public sealed record UpdateJobExecutionResultParameters(JobExecutionState State, string? Tag, DateTime EndUtc);

