namespace Mews.Job.Scheduler.Features.Jobs.Dtos;

public sealed class JobUpdateResultDto
{
    public required IEnumerable<JobDto> UpdatedJobs { get; init; }
}
