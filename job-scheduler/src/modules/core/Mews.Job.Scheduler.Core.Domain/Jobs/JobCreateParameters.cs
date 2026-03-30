using Mews.Job.Scheduler.BuildingBlocks.Types.DateTimeSpan;

namespace Mews.Job.Scheduler.Domain.Jobs;

public sealed class JobCreateParameters
{
    public JobCreateParameters(
        DateTime startUtc,
        string executorTypeName,
        string team,
        DateTimeSpan maxExecutionTime,
        Guid creatorProfileId,
        string? name = null,
        DateTimeSpan? period = null,
        JobOptions? options = null,
        string? data = null)
    {
        Name = name;
        ExecutorTypeName = executorTypeName;
        Team = team;
        StartUtc = startUtc;
        Period = period;
        MaxExecutionTime = maxExecutionTime;
        Options = options ?? default(JobOptions);
        Data = data;
        CreatorProfileId = creatorProfileId;
    }

    public string? Name { get; }

    public string ExecutorTypeName { get; }

    public string Team { get; }

    public DateTime StartUtc { get; }

    public DateTimeSpan? Period { get; }

    public DateTimeSpan MaxExecutionTime { get; }

    public JobOptions Options { get; }

    public string? Data { get; }

    public Guid CreatorProfileId { get; }
}
