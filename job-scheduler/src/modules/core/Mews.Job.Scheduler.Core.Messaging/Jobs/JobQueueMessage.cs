namespace Mews.Job.Scheduler.Core.Messaging.Jobs;

// The type is temporary and needs to be synced with monolith's type for deserialization purposes. The type will be replaced
// once Job Scheduling SDK is exported.
// See more: https://github.com/MewsSystems/mews/blob/develop/src/Framework/Library/Mews.Library.JobProcessing/Messages/JobQueueMessage.cs#L5
public sealed record JobQueueMessage(
    string Identifier,
    string? Name,
    Guid JobId,
    string ExecutorType,
    string Team,
    DateTimeOffset CreatedUtc,
    Guid CreatorProfileId,
    DateTimeOffset UpdatedUtc,
    Guid UpdaterProfileId,
    DateTimeOffset ScheduledExecutionStart,
    TimeSpan JobMaxExecutionTime,
    DateTimeOffset? ExecutionStart,
    DateTimeOffset? PreviousSuccessfulStart,
    string LogVerbosity,
    string State,
    IEnumerable<string> Options,
    string? Data,
    string? Period
);

public sealed record JobQueueMessageIdentifier(Guid JobId, DateTime StartUtc)
{
    public string Value { get; } = $"{JobId:N}_{StartUtc:yyyyMMddHHmmssfff}";
}
