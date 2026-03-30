using System.Diagnostics;
using MediatR;
using Mews.Atlas.OpenTelemetry;
using Mews.Job.Scheduler.BuildingBlocks.Types.Extensions;
using Mews.Job.Scheduler.Core.Application.Helpers;
using Mews.Job.Scheduler.Core.Application.JobProcessing.Commands;
using Mews.Job.Scheduler.Domain;
using Mews.Job.Scheduler.Domain.DateTimeProviders;
using Mews.Job.Scheduler.Domain.JobExecutions;
using Mews.Job.Scheduler.Domain.JobLifecycle;
using Mews.Job.Scheduler.Domain.Jobs;
using Mews.Job.Scheduler.Domain.Profiles;
using Mews.Job.Scheduler.Job;
using Mews.Job.Scheduler.Observability;
using Microsoft.Extensions.Logging;

namespace Mews.Job.Scheduler.Core.Application.JobProcessing.Handlers;

public sealed class ConfirmResultHandler : IRequestHandler<ConfirmResultCommand>
{
    private readonly IJobPersistence _jobPersistence;
    private readonly JobTimeoutRetryCache _timeoutRetryCache;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly SystemProfile _systemProfile;
    private readonly ILogger<ConfirmResultHandler> _logger;

    public ConfirmResultHandler(
        IJobPersistence jobPersistence,
        JobTimeoutRetryCache timeoutRetryCache,
        IDateTimeProvider dateTimeProvider,
        SystemProfile systemProfile,
        ILogger<ConfirmResultHandler> logger)
    {
        _jobPersistence = jobPersistence;
        _timeoutRetryCache = timeoutRetryCache;
        _dateTimeProvider = dateTimeProvider;
        _systemProfile = systemProfile;
        _logger = logger;
    }

    public async Task Handle(ConfirmResultCommand request, CancellationToken cancellationToken)
    {
        var currentActivity = Activity.Current;
        
        try
        {
            var jobExecution = await _jobPersistence.JobExecutions.GetRequiredByIdAsync(request.JobExecutionId, cancellationToken);
            if (jobExecution.IsProcessed)
            {
                return;
            }
            
            var job = await _jobPersistence.Jobs.GetRequiredByIdAsync(jobExecution.JobId, cancellationToken);

            var nowUtc = _dateTimeProvider.NowUtc;
            var confirmJobExecutionResultParameters = new UpdateJobExecutionResultParameters(request.State, request.Tag, nowUtc);
            jobExecution.UpdateJobExecutionResult(confirmJobExecutionResultParameters, nowUtc, _systemProfile);

            var retryCount = jobExecution.IsTimeout
                ? _timeoutRetryCache.Increment(job.Id)
                : _timeoutRetryCache.Reset(job.Id);

            var jobResetParameters = new ConfirmJobAfterExecutionParameters(jobExecution.IsSuccess, jobExecution.IsTimeout, retryCount, request.DeleteJob, request.FutureRunData);
            job.ConfirmAfterExecution(jobResetParameters, nowUtc, _systemProfile);

            if (!job.IsPeriodical || request.DeleteJob)
            {
                job.SoftDelete(nowUtc, _systemProfile.Id);
            }

            await _jobPersistence.SaveChangesAsync(cancellationToken);
        }
        catch (StateTransitionException stateTransitionException)
        {
            // Exception occurs when JobTimeoutHandler has handled the job execution before this handler.
            currentActivity?.RecordExceptionWithStatus(stateTransitionException);
            _logger.LogWarning(
                exception: stateTransitionException,
                message: "State transition exception occurred while confirming job execution result from {FromState} to {ToState} for {ExecutionId}.",
                stateTransitionException.FromState, stateTransitionException.ToState, request.JobExecutionId
            );
        }
        catch (ConcurrencyException concurrencyException)
        {
            // Exception occurs when the job execution has been deleted by Job Timeout handler.
            currentActivity?.RecordExceptionWithStatus(concurrencyException);
            _logger.LogWarning(
                exception: concurrencyException,
                message: "Concurrency exception occurred while confirming job execution result for {ExecutionId}.",
                request.JobExecutionId
            );
        }
        catch (Exception exception)
        {
            JobSchedulerServiceExceptionMapper.MapToProcessingException(exception).Throw();
        }
    }
}
