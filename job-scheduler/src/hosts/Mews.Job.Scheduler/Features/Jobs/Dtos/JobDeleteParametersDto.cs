namespace Mews.Job.Scheduler.Features.Jobs.Dtos;

public sealed class JobDeleteParametersDto
{
    public required Guid UpdaterProfileId { get; init; }
}

public sealed class JobDeleteBatchParametersDto
{
    public required IEnumerable<Guid> JobIds { get; init; }

    public required Guid UpdaterProfileId { get; init; }
}
