using MediatR;
using Mews.Job.Scheduler.Core.Application.Helpers;
using Mews.Job.Scheduler.Core.Application.Jobs.Commands;
using Mews.Job.Scheduler.Domain;

namespace Mews.Job.Scheduler.Core.Application.Jobs.Handlers;

public sealed class JobGetHandler : IRequestHandler<JobGetCommand, Domain.Jobs.Job>
{
    private readonly IJobPersistence _jobPersistence;

    public JobGetHandler(IJobPersistence jobPersistence)
    {
        _jobPersistence = jobPersistence;
    }
    public async Task<Domain.Jobs.Job> Handle(JobGetCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return await _jobPersistence.Jobs.GetRequiredByIdAsync(request.Id, includeExecutor: true, cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            JobSchedulerServiceExceptionMapper.MapToSchedulerServiceException(exception).Throw();
            return default;
        }
    }
}
