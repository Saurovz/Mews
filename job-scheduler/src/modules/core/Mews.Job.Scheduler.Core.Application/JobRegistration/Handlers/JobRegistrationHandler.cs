using System.Collections.Immutable;
using MediatR;
using Mews.Job.Scheduler.Core.Application.Helpers;
using Mews.Job.Scheduler.Core.Application.Registration;
using Mews.Job.Scheduler.Domain;
using Mews.Job.Scheduler.Domain.DateTimeProviders;
using Mews.Job.Scheduler.Domain.Executors;
using Mews.Job.Scheduler.Domain.Jobs;
using Microsoft.Extensions.Logging;

namespace Mews.Job.Scheduler.Core.Application.JobRegistration.Handlers;

public sealed class JobRegistrationHandler : IRequestHandler<JobRegistrationCommand, JobRegistrationResult>
{
    private readonly IDateTimeProvider _dateTime;
    private readonly IJobPersistence _jobPersistence;
    private readonly IExecutorRepository _executorRepository;
    private readonly ILogger<JobRegistrationHandler> _logger;

    public JobRegistrationHandler(
        IDateTimeProvider dateTime,
        IJobPersistence jobPersistence,
        IExecutorRepository executorRepository,
        ILogger<JobRegistrationHandler> logger)
    {
        _dateTime = dateTime;
        _jobPersistence = jobPersistence;
        _executorRepository = executorRepository;
        _logger = logger;
    }

    public async Task<JobRegistrationResult> Handle(JobRegistrationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var types = request.JobsToRegister.Select(j => j.ExecutorTypeName).Order().ToList();
            _logger.LogInformation("[registration] Recognized executors: {executors}.", request.RecognizedExecutorsMetadata.Select(p => p.Type));
            _logger.LogInformation("[registration] Jobs to register: {executors}.", types);

            var nowUtc = _dateTime.NowUtc;
            var synchronizedExecutors = await SynchronizeExecutorsAsync(request, nowUtc, cancellationToken);
            var createdJobs = await RegisterNewJobsAsync(synchronizedExecutors, request.JobsToRegister, nowUtc, cancellationToken);
            var deletedJobs = await DeleteOldJobsAsync(synchronizedExecutors, request.UpdaterProfileId, nowUtc, cancellationToken);

            await _jobPersistence.SaveChangesAsync(cancellationToken);
            
            return new JobRegistrationResult
            {
                CreatedJobs = createdJobs,
                DeletedJobs = deletedJobs
            };
        }
        catch (Exception exception)
        {
            JobSchedulerServiceExceptionMapper.MapToSchedulerServiceException(exception).Throw();
            return default;
        }
    }

    private async Task<IEnumerable<Domain.Jobs.Job>> RegisterNewJobsAsync(
        SynchronizedExecutors synchronizedExecutors,
        IEnumerable<JobCreateParameters> recognizedJobs,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        var addedExecutorTypes = synchronizedExecutors.AddedExecutors.Select(e => e.Type);
        var jobsToRegister = recognizedJobs.Where(j => addedExecutorTypes.Contains(j.ExecutorTypeName));
        var createdJobs = jobsToRegister.Select(j =>
        {
            var executor = synchronizedExecutors.AddedExecutors.Single(e => e.Type == j.ExecutorTypeName);
            var job = Domain.Jobs.Job.Create(j, executor, nowUtc);
            _logger.LogInformation("[registration] Added {ExecutorTypeName} ({JobId}).", executor.Type, job.Id);

            return job;
        }).ToList();

        await _jobPersistence.AddRangeAsync(createdJobs, cancellationToken);

        return createdJobs;
    }

    private async Task<IEnumerable<Domain.Jobs.Job>> DeleteOldJobsAsync(
        SynchronizedExecutors synchronizedExecutors,
        Guid updaterProfileId,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        var removedExecutorTypes = synchronizedExecutors.RemovedExecutors.Select(e => e.Type).ToImmutableHashSet();
        var jobsToUnregister = await _jobPersistence.Jobs.GetUnregisteredJobs(removedExecutorTypes, cancellationToken);

        foreach (var job in jobsToUnregister)
        {
            job.SoftDelete(nowUtc, updaterProfileId);
            _logger.LogInformation("[registration] Removed {ExecutorTypeName} ({JobId}).", job.Executor.Type, job.Id);
        }

        return jobsToUnregister;
    }

    private async Task<SynchronizedExecutors> SynchronizeExecutorsAsync(
        JobRegistrationCommand request,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        var registeredExecutors = await _executorRepository.GetExecutorsAsync(cancellationToken);
        SynchronizeTeams(request, registeredExecutors);
        
        var addedExecutors = await AddExecutorsAsync(request, registeredExecutors, cancellationToken);
        var removedExecutors = RemoveExecutors(request, registeredExecutors, nowUtc);
        var synchronizedExecutors = registeredExecutors.Concat(addedExecutors).ToList();

        foreach(var addedExecutor in addedExecutors)
        {
            _logger.LogInformation("[registration] Added executor {ExecutorType}.", addedExecutor.Type);
        }
        foreach(var removedExecutor in removedExecutors)
        {
            _logger.LogInformation("[registration] Removed executor {ExecutorType}.", removedExecutor.Type);
        }

        return new SynchronizedExecutors(addedExecutors, removedExecutors, synchronizedExecutors);
    }
    
    private void SynchronizeTeams(JobRegistrationCommand request, IReadOnlyList<Executor> registeredExecutors)
    {
        foreach (var executor in registeredExecutors)
        {
            var existingExecutor = request.RecognizedExecutorsMetadata.SingleOrDefault(e => e.Type == executor.Type && e.Team != executor.Team);
            if (existingExecutor != null)
            {
                executor.UpdateTeam(existingExecutor.Team);
            }
        }
    }
    
    private async Task<IReadOnlyList<Executor>> AddExecutorsAsync(JobRegistrationCommand request, IReadOnlyList<Executor> registeredExecutors, CancellationToken cancellationToken)
    {
        var registeredExecutorTypes = registeredExecutors.Select(e => e.Type).ToList();
        var executorsToAdd = request.RecognizedExecutorsMetadata
            .Where(p => !registeredExecutorTypes.Contains(p.Type));
        
        var createdExecutors = executorsToAdd.Select(p => Executor.Create(p.Type, p.Team)).ToList();
        await _executorRepository.AddRangeAsync(createdExecutors, cancellationToken);

        var recognizedExecutorTypes = request.RecognizedExecutorsMetadata.Select(p => p.Type).ToList();
        var executorsToRestore = registeredExecutors.Where(e => e.DeletedUtc != null && recognizedExecutorTypes.Contains(e.Type)).ToList();
        executorsToRestore.ForEach(e => e.Restore());

        return createdExecutors.Concat(executorsToRestore).ToList();
    }

    private IReadOnlyList<Executor> RemoveExecutors(JobRegistrationCommand request, IReadOnlyList<Executor> registeredExecutors, DateTime nowUtc)
    {
        var recognizedExecutorTypes = request.RecognizedExecutorsMetadata.Select(p => p.Type).ToList();
        var executorsToDelete = registeredExecutors.Where(e => !recognizedExecutorTypes.Contains(e.Type) && e.DeletedUtc == null).ToList();
        executorsToDelete.ForEach(e => e.Delete(nowUtc));
        
        return executorsToDelete;
    }
}
