using Mews.Job.Scheduler.BuildingBlocks.Application.CQS;
using Mews.Job.Scheduler.Domain.Jobs;

namespace Mews.Job.Scheduler.Core.Application.Jobs.Commands;

public sealed class JobCreateCommand : ICommand<IReadOnlyList<Domain.Jobs.Job>>
{
    public required IReadOnlyList<JobCreateParameters> CreateParameters { get; init; }
}
