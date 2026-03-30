using MediatR;
using Mews.Job.Scheduler.Core.Application.Helpers;
using Mews.Job.Scheduler.Core.Application.Jobs.Commands;
using Mews.Job.Scheduler.Domain;
using Mews.Job.Scheduler.Domain.DateTimeProviders;
using Mews.Job.Scheduler.Domain.Executors;
using Mews.Job.Scheduler.Domain.Jobs;
using Mews.Job.Scheduler.Domain.Profiles;

namespace Mews.Job.Scheduler.Core.Application.Jobs.Handlers;

public sealed class JobCreateHandler : IRequestHandler<JobCreateCommand, IReadOnlyList<Domain.Jobs.Job>>
{
    private readonly IDateTimeProvider _dateTime;
    private readonly IJobPersistence _jobPersistence;
    private readonly IExecutorRepository _executorRepository;

    public JobCreateHandler(IDateTimeProvider dateTime, IJobPersistence jobPersistence, IExecutorRepository executorRepository)
    {
        _dateTime = dateTime;
        _jobPersistence = jobPersistence;
        _executorRepository = executorRepository;
    }

    public async Task<IReadOnlyList<Domain.Jobs.Job>> Handle(JobCreateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var createdUtc = _dateTime.NowUtc;
            var executors = await GetExecutorsAsync(request.CreateParameters, cancellationToken);
            var jobs = request.CreateParameters.Select(j =>
            {
                var executor = executors.Single(e => e.Type == j.ExecutorTypeName);
                return Domain.Jobs.Job.Create(j, executor, createdUtc);
            }).ToList();

            await _jobPersistence.AddRangeAsync(jobs, cancellationToken);
            await _jobPersistence.SaveChangesAsync(cancellationToken);

            return jobs;
        }
        catch (Exception exception)
        {
            JobSchedulerServiceExceptionMapper.MapToSchedulerServiceException(exception).Throw();
            return default;
        }
    }
    
    private async Task<IReadOnlyList<Executor>> GetExecutorsAsync(IReadOnlyList<JobCreateParameters> jobParameters, CancellationToken cancellationToken)
    {
        var executorsToAdd = new List<Executor>();
        
        var jobExecutorTypes = jobParameters.Select(j => j.ExecutorTypeName).ToList();
        var executor = await _executorRepository.GetExecutorByTypesAsync(jobExecutorTypes, cancellationToken);
        var executorTypes = executor.Select(e => e.Type);
        var jobParametersWithoutExecutor = jobParameters.Where(p => !executorTypes.Contains(p.ExecutorTypeName)).ToList();
        foreach (var parameter in jobParametersWithoutExecutor)
        {
            executorsToAdd.Add(Executor.Create(parameter.ExecutorTypeName, parameter.Team));
        }

        await _executorRepository.AddRangeAsync(executorsToAdd, cancellationToken);
        
        return executor.Concat(executorsToAdd).ToList();
    }
}
