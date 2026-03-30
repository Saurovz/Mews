using System.ComponentModel.DataAnnotations;
using Mews.Job.Scheduler.BuildingBlocks.Types.DateTimeSpan;
using NJsonSchema;
using NJsonSchema.Annotations;

namespace Mews.Job.Scheduler.Features.Jobs.Dtos;

public sealed class JobCreateParametersDto
{
    public required IEnumerable<JobCreateDataDto> Jobs { get; init; }
}

public sealed class JobCreateDataDto
{
    public string? Name { get; init; }

    public required string ExecutorTypeName { get; init; }

    public required string Team { get; init; }

    public required DateTime StartUtc { get; init; }

    public DateTimeSpan? Period { get; init; }

    public required DateTimeSpan MaxExecutionTime { get; init; }
    
    [JsonSchema(JsonObjectType.Array, ArrayItem = typeof(JobOptionsDto))]
    public JobOptionsDto? Options { get; init; }

    public string? Data { get; init; }

    public required Guid CreatorProfileId { get; init; }
}
