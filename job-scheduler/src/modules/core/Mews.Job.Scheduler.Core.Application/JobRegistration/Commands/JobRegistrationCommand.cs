using System.Collections.Immutable;
using Mews.Job.Scheduler.BuildingBlocks.Application.CQS;
using Mews.Job.Scheduler.Domain.Executors;
using Mews.Job.Scheduler.Domain.Jobs;

namespace Mews.Job.Scheduler.Core.Application.Registration;

public sealed class JobRegistrationCommand : ICommand<JobRegistrationResult>
{
    public required IReadOnlyList<ExecutorCreateParameters> RecognizedExecutorsMetadata { get; init; }

    public required IReadOnlyList<JobCreateParameters> JobsToRegister { get; init; }

    public required Guid UpdaterProfileId { get; init; }
}
