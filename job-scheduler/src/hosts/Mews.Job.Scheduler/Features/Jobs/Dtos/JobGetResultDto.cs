namespace Mews.Job.Scheduler.Features.Jobs.Dtos;

public sealed class JobGetResultDto
{
    public required IEnumerable<JobDto> Jobs { get; init; }
}
