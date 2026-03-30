using System.Diagnostics;
using MediatR;
using Mews.Job.Scheduler.BuildingBlocks.Types.Extensions;
using Mews.Job.Scheduler.Core.Application.Helpers;
using Mews.Job.Scheduler.Core.Application.JobProcessing.Commands;
using Mews.Job.Scheduler.Domain;
using Mews.Job.Scheduler.Domain.DateTimeProviders;
using Mews.Job.Scheduler.Domain.JobExecutions;
using Mews.Job.Scheduler.Domain.Profiles;
using Mews.Job.Scheduler.Job;
using Mews.Job.Scheduler.Observability;

namespace Mews.Job.Scheduler.Core.Application.JobProcessing.Handlers;

public sealed class ConfirmProcessingHandler : IRequestHandler<ConfirmProcessingCommand, JobExecution>
{
    private readonly IJobPersistence _jobPersistence;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly SystemProfile _systemProfile;
    
    public ConfirmProcessingHandler(IJobPersistence jobPersistence, IDateTimeProvider dateTimeProvider, SystemProfile systemProfile)
    {
        _jobPersistence = jobPersistence;
        _dateTimeProvider = dateTimeProvider;
        _systemProfile = systemProfile;
    }
    
    public async Task<JobExecution> Handle(ConfirmProcessingCommand request, CancellationToken cancellationToken)
    {
        using var activity = JobProcessingDiagnostics.Source.StartActivity(parentContext: Activity.Current?.Context);

        try
        {
            var nowUtc = _dateTimeProvider.NowUtc;
            var job = await _jobPersistence.Jobs.GetRequiredByIdAsync(request.JobId, includeExecutor: true, cancellationToken: cancellationToken);

            var isMarkedAsInProgress = job.TryMarkAsInProgress(nowUtc, _systemProfile);

            var jobExecution =  isMarkedAsInProgress ? 
                await CreateJobExecution(job, request.ExecutionTransactionIdentifier, nowUtc, cancellationToken) : 
                await _jobPersistence.JobExecutions.GetByJobIdWithTransactionIdentifier(job.Id, request.ExecutionTransactionIdentifier, cancellationToken);

            if (jobExecution == null)
            {
                JobProcessingDiagnostics.AddJobTimeOutCheckEvent(activity, job, nowUtc);
                throw new EntityNotFoundException($"Entity {nameof(JobExecution)} was not found with JobId: {job.Id} and TransactionIdentifier: {request.ExecutionTransactionIdentifier}");
            }

            return jobExecution;
        }
        catch (Exception exception)
        {
            JobSchedulerServiceExceptionMapper.MapToProcessingException(exception).Throw();
            return default;
        }
    }

    private async Task<JobExecution> CreateJobExecution(Domain.Jobs.Job job, string transactionIdentifier, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var jobExecutionCreateParameters = new JobExecutionCreateParameters(job.Id, job.Executor.Type, nowUtc, transactionIdentifier);
        var jobExecution = JobExecution.Create(jobExecutionCreateParameters, nowUtc, _systemProfile);
        await _jobPersistence.AddAsync(jobExecution, cancellationToken);

        await _jobPersistence.SaveChangesAsync(cancellationToken);
                
        return jobExecution;
    }
}
