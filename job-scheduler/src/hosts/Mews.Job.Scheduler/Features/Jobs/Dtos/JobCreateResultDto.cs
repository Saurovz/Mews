namespace Mews.Job.Scheduler.Features.Jobs.Dtos;

public sealed class JobCreateResultDto
{
    public required IEnumerable<JobDto> CreatedJobs { get; init; }
}
