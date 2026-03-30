using MediatR;
using Mews.Job.Scheduler.Core.Application.Helpers;
using Mews.Job.Scheduler.Core.Application.Jobs.Commands;
using Mews.Job.Scheduler.Domain;

namespace Mews.Job.Scheduler.Core.Application.Jobs.Handlers;

public sealed class JobGetFilteredHandler : IRequestHandler<JobGetFilteredCommand, IEnumerable<Domain.Jobs.Job>>
{
    private readonly IJobPersistence _jobPersistence;

    public JobGetFilteredHandler(IJobPersistence jobPersistence)
    {
        _jobPersistence = jobPersistence;
    }

    public async Task<IEnumerable<Domain.Jobs.Job>> Handle(JobGetFilteredCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return await _jobPersistence.Jobs.GetFilteredAsync(request.Filters, cancellationToken);
        }
        catch (Exception exception)
        {
            JobSchedulerServiceExceptionMapper.MapToSchedulerServiceException(exception).Throw();
            return default;
        }
    }
}
