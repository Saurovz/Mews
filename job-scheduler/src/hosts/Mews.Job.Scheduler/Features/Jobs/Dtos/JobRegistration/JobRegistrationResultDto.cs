namespace Mews.Job.Scheduler.Features.Jobs.Dtos;

public sealed class JobRegistrationResultDto
{
    public required IEnumerable<JobDto> CreatedJobs { get; init; }

    public required IEnumerable<JobDto> DeletedJobs { get; init; }
}
