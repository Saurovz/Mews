using Mews.Job.Scheduler.BuildingBlocks.Types.DateTimeSpan;
using NJsonSchema;
using NJsonSchema.Annotations;

namespace Mews.Job.Scheduler.Features.Jobs.Dtos;

public sealed class JobUpdateParametersDto
{
    public required JobUpdateDataDto UpdatedJob { get; init; }
}

public sealed class JobBatchUpdateParametersDto
{
    public required IDictionary<Guid, JobUpdateDataDto> UpdatedJobs { get; init; }
}

public sealed class JobUpdateDataDto
{
    public required string? Name { get; init; }

    public required string ExecutorTypeName { get; init; }

    public required string Team { get; init; }

    public required DateTime StartUtc { get; init; }

    public required DateTimeSpan? Period { get; init; }

    public required DateTimeSpan MaxExecutionTime { get; init; }
    
    [JsonSchema(JsonObjectType.Array, ArrayItem = typeof(JobOptionsDto))]
    public required JobOptionsDto Options { get; init; }

    public required string? Data { get; init; }

    public required Guid UpdaterProfileId { get; init; }
}
