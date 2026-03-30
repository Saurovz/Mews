using MediatR;
using Mews.Job.Scheduler.Core.Application.Helpers;
using Mews.Job.Scheduler.Core.Application.Jobs.Commands;
using Mews.Job.Scheduler.Domain;
using Mews.Job.Scheduler.Domain.DateTimeProviders;
using Mews.Job.Scheduler.Domain.Executors;
using Mews.Job.Scheduler.Domain.Jobs;

namespace Mews.Job.Scheduler.Core.Application.Jobs.Handlers;

public sealed class JobUpdateHandler : IRequestHandler<JobUpdateCommand, IReadOnlyList<Domain.Jobs.Job>>
{
    private readonly IDateTimeProvider _dateTime;
    private readonly IJobPersistence _jobPersistence;
    private readonly IExecutorRepository _executorRepository;

    public JobUpdateHandler(
        IDateTimeProvider dateTime, 
        IJobPersistence jobPersistence,
        IExecutorRepository executorRepository)
    {
        _dateTime = dateTime;
        _jobPersistence = jobPersistence;
        _executorRepository = executorRepository;
    }

    public async Task<IReadOnlyList<Domain.Jobs.Job>> Handle(JobUpdateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var updatedUtc = _dateTime.NowUtc;
            var jobs = await _jobPersistence.Jobs.GetByIdsAsync(request.JobUpdates.Keys.ToList(), includeExecutor: true, cancellationToken: cancellationToken);

            foreach (var job in jobs)
            {
                var updateParameters = request.JobUpdates[job.Id];
                var executor = await GetExecutor(job, updateParameters, cancellationToken);   
                job.Update(updateParameters, executor, updatedUtc);
            }

            await _jobPersistence.SaveChangesAsync(cancellationToken);

            return jobs;
        }
        catch (Exception exception)
        {
            JobSchedulerServiceExceptionMapper.MapToSchedulerServiceException(exception).Throw();
            return default;
        }
    }

    private async Task<Executor> GetExecutor(Domain.Jobs.Job job, JobUpdateParameters updateParameters, CancellationToken cancellationToken)
    {
        var executorToUpdateTo = updateParameters.ExecutorTypeName;
        if (job.Executor.Type == executorToUpdateTo)
        {
            return job.Executor;
        }
        
        return await _executorRepository.GetRequiredExecutorByTypeAsync(executorToUpdateTo, cancellationToken);
    }
}
