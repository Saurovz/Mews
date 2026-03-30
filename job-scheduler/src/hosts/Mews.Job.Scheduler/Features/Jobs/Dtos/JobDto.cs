using System.Text.Json.Serialization;
using Mews.Job.Scheduler.BuildingBlocks.Types.DateTimeSpan;
using NJsonSchema;
using NJsonSchema.Annotations;

namespace Mews.Job.Scheduler.Features.Jobs.Dtos;

public sealed class JobDto
{
    public required Guid Id { get; init; }

    public string? Name { get; init; }

    public required string ExecutorTypeName { get; init; }

    public required string? Team { get; init; }

    public required DateTime StartUtc { get; init; }

    public DateTime? PreviousSuccessfulStartUtc { get; init; }

    public DateTimeSpan? Period { get; init; }

    public required DateTimeSpan MaxExecutionTime { get; init; }
    
    public required DateTime? ExecutionStartUtc { get; init; }

    public required JobStateDto Status { get; init; }

    [JsonSchema(JsonObjectType.Array, ArrayItem = typeof(JobOptionsDto))]
    public required JobOptionsDto Options { get; init; }
    
    public string? Data { get; init; }

    public required DateTime CreatedUtc { get; init; }

    public required DateTime UpdatedUtc { get; init; }

    public required Guid CreatorProfileId { get; init; }

    public required Guid UpdaterProfileId { get; init; }

    public required bool IsDeleted { get; init; }

    public DateTime? DeletedUtc { get; init; }
}
