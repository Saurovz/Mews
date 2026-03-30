namespace Mews.Job.Scheduler.Features.Jobs.Dtos;

public sealed class JobRegistrationParametersDto
{
    public required IEnumerable<JobExecutorMetadataDto> RecognizedJobExecutorsMetadata { get; init; }

    public required IEnumerable<JobCreateDataDto> JobsToRegister { get; init; }

    public required Guid UpdaterProfileId { get; init; }
}
