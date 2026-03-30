using Mews.Job.Scheduler.BuildingBlocks.Application.CQS;

namespace Mews.Job.Scheduler.Core.Application.Jobs.Commands;

public sealed class JobDeleteCommand : ICommand
{
    public required IReadOnlyList<Guid> Ids { get; init; }

    public required Guid UpdaterProfileId { get; init; }
}
