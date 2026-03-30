using Mews.Job.Scheduler.BuildingBlocks.Types.DateTimeSpan;

namespace Mews.Job.Scheduler.Domain.Jobs;

public sealed record JobUpdateParameters(
    string? Name,
    string ExecutorTypeName,
    DateTime StartUtc,
    DateTimeSpan? Period,
    DateTimeSpan MaxExecutionTime,
    JobOptions Options,
    string? Data,
    Guid UpdaterProfileId
);
