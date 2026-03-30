using MediatR;
using Mews.Job.Scheduler.Core.Application.Helpers;
using Mews.Job.Scheduler.Core.Application.JobExecutions.Commands;
using Mews.Job.Scheduler.Domain;
using Mews.Job.Scheduler.Domain.JobExecutions;

namespace Mews.Job.Scheduler.Core.Application.JobExecutions.Handlers;

public sealed class JobExecutionGetFilteredHandler : IRequestHandler<JobExecutionGetFilteredCommand, IEnumerable<JobExecution>>
{
    private readonly IJobPersistence _jobPersistence;
    
    public JobExecutionGetFilteredHandler(IJobPersistence jobPersistence)
    {
        _jobPersistence = jobPersistence;
    }
    
    public async Task<IEnumerable<JobExecution>> Handle(JobExecutionGetFilteredCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return await _jobPersistence.JobExecutions.GetFilteredAsync(request.Filters, cancellationToken);
        }
        catch (Exception exception)
        {
            JobSchedulerServiceExceptionMapper.MapToSchedulerServiceException(exception).Throw();
            return default;
        }
    }
}
