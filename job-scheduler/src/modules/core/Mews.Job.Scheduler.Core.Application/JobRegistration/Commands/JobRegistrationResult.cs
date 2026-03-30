namespace Mews.Job.Scheduler.Core.Application.Registration;

public sealed class JobRegistrationResult
{
    public required IEnumerable<Domain.Jobs.Job> CreatedJobs { get; init; }

    public required IEnumerable<Domain.Jobs.Job> DeletedJobs { get; init; }
}
