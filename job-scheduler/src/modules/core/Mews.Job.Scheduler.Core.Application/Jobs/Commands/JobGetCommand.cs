using Mews.Job.Scheduler.BuildingBlocks.Application.CQS;

namespace Mews.Job.Scheduler.Core.Application.Jobs.Commands;

public sealed class JobGetCommand : ICommand<Domain.Jobs.Job>
{
    public required Guid Id { get; init; }
}
