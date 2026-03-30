using MediatR;

namespace Mews.Job.Scheduler.BuildingBlocks.Application.CQS;

public interface ICommand<out TCommandResult> : IRequest<TCommandResult>
{
}


public interface ICommand : ICommand<Unit>, IRequest
{
}
