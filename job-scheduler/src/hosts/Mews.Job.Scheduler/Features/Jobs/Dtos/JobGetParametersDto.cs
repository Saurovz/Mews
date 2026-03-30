using NJsonSchema;
using NJsonSchema.Annotations;

namespace Mews.Job.Scheduler.Features.Jobs.Dtos;

public sealed class JobGetParametersDto
{
    public IEnumerable<Guid>? Ids { get; init; }
    
    public string? Name { get; init; }

    public IEnumerable<string>? ExecutorTypeNames { get; init; }
    
    [JsonSchema(JsonObjectType.Array, ArrayItem = typeof(JobStatesDto))]
    public JobStatesDto? States { get; init; }

    public DateTime? StartUtc { get; init; }

    public DateTime? EndUtc { get; init; }

    public bool ShowDeleted { get; init; }

    public LimitationDto? Limitation { get; init; }
}
