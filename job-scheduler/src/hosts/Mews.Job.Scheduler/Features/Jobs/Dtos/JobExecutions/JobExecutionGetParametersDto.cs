using NJsonSchema;
using NJsonSchema.Annotations;

namespace Mews.Job.Scheduler.Features.Jobs.Dtos;

public class JobExecutionGetParametersDto
{
    public IEnumerable<Guid>? Ids { get; init; }

    public IEnumerable<Guid>? JobIds { get; init; }

    [JsonSchema(JsonObjectType.Array, ArrayItem = typeof(JobExecutionStatesDto))]
    public JobExecutionStatesDto? States { get; init; }

    public DateTimeIntervalDto? StartInterval { get; init; }

    public IEnumerable<string>? ExecutorTypeNames { get; init; }

    public bool ShowDeleted { get; init; }

    public required LimitationDto Limitation { get; init; }
}
