using MediatR;
using Mews.Job.Scheduler.Core.Application.Helpers;
using Mews.Job.Scheduler.Core.Application.Jobs.Commands;
using Mews.Job.Scheduler.Domain;
using Mews.Job.Scheduler.Domain.DateTimeProviders;
using Mews.Job.Scheduler.Domain.Profiles;

namespace Mews.Job.Scheduler.Core.Application.Jobs.Handlers;

public sealed class JobDeleteHandler : IRequestHandler<JobDeleteCommand>
{
    private readonly IDateTimeProvider _dateTime;
    private readonly IJobPersistence _jobPersistence;

    public JobDeleteHandler(
        IDateTimeProvider dateTime,
        IJobPersistence jobPersistence)
    {
        _dateTime = dateTime;
        _jobPersistence = jobPersistence;
    }

    public async Task Handle(JobDeleteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var jobs = await _jobPersistence.Jobs.GetByIdsAsync(request.Ids, includeExecutor: false, cancellationToken);
            var nowUtc = _dateTime.NowUtc;
            foreach (var job in jobs.Where(job => !job.IsDeleted))
            {
                job.SoftDelete(nowUtc, request.UpdaterProfileId);
            }

            await _jobPersistence.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            JobSchedulerServiceExceptionMapper.MapToSchedulerServiceException(exception).Throw();
        }
    }
}
