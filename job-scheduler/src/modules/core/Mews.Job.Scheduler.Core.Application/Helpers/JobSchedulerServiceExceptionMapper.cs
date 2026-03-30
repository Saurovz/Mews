using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using Mews.Job.Scheduler.Job;

[assembly: InternalsVisibleTo("Mews.Job.Scheduler.UnitTests")]

namespace Mews.Job.Scheduler.Core.Application.Helpers;

internal static class JobSchedulerServiceExceptionMapper
{
    public static ExceptionDispatchInfo MapToSchedulerServiceException(Exception exception)
    {
        var dispatchInfo = ExceptionDispatchInfo.Capture(exception);
        return Map(dispatchInfo, reason => new JobSchedulerServiceException(reason, dispatchInfo.SourceException));
    }
    
    public static ExceptionDispatchInfo MapToProcessingException(Exception exception)
    {
        var dispatchInfo = ExceptionDispatchInfo.Capture(exception);
        return Map(dispatchInfo, reason => new JobProcessingException(reason, dispatchInfo.SourceException));
    }

    private static ExceptionDispatchInfo Map<TException>(ExceptionDispatchInfo exceptionDispatchInfo, Func<JobSchedulerServiceExceptionReason, TException> exceptionToCapture)
        where TException : JobSchedulerServiceException
    {
        var exception = exceptionDispatchInfo.SourceException;
        if (exception is OperationCanceledException)
        {
            return exceptionDispatchInfo;
        }

        return ExceptionDispatchInfo.Capture(exceptionToCapture(GetReason(exception)));
    }
    
    private static JobSchedulerServiceExceptionReason GetReason(Exception exception)
    {
        return exception switch
        {
            EntityNotFoundException => JobSchedulerServiceExceptionReason.EntityNotFound,
            StateTransitionException => JobSchedulerServiceExceptionReason.EntityInvalidStateTransition,
            EntityProcessingException => JobSchedulerServiceExceptionReason.EntityProcessingConflict,
            TransientPersistenceFaultException => JobSchedulerServiceExceptionReason.ServiceCommunicationProblem,
            _ => JobSchedulerServiceExceptionReason.InternalError
        };
    }
}
