namespace Mews.Job.Scheduler.Features.Jobs.Dtos;

public sealed class JobExecutionDto
{
    public required Guid Id { get; init; }

    public required JobDto Job { get; init; }

    public required JobExecutionStateDto State { get; init; }

    public required DateTime StartUtc { get; init; }

    public DateTime? EndUtc { get; init; }

    public required string? TransactionIdentifier { get; init; }

    public string? Tag { get; init; }

    public string? ExecutorTypeNameValue { get; init; }

    public required DateTime CreatedUtc { get; init; }

    public required DateTime UpdatedUtc { get; init; }

    public DateTime? DeletedUtc { get; init; }

    public required Guid CreatorProfileId { get; init; }

    public required Guid UpdaterProfileId { get; init; }

    public required bool IsDeleted { get; init; }
}
