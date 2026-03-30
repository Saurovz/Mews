using Mews.Job.Scheduler;
using Mews.Job.Scheduler.Core.Application.Helpers;
using Mews.Job.Scheduler.Domain.Jobs;
using Mews.Job.Scheduler.Job;

namespace Mews.Job.Scheduler.UnitTests;

[TestFixture]
public class JobSchedulerServiceExceptionMapperTests
{
    [Test]
    public void MapToSchedulerServiceException_ReturnsJobSchedulerServiceException()
    {
        var exception = new Exception();

        var sut = JobSchedulerServiceExceptionMapper.MapToSchedulerServiceException(exception);

        Assert.That(sut.SourceException, Is.InstanceOf<JobSchedulerServiceException>());
    }

    [Test]
    public void MapToProcessingException_ReturnsJobProcessingException()
    {
        var exception = new Exception();

        var sut = JobSchedulerServiceExceptionMapper.MapToProcessingException(exception);

        Assert.That(sut.SourceException, Is.InstanceOf<JobProcessingException>());
    }

    [Test]
    public void MapToSchedulerServiceException_WhenOperationCanceledException_ReturnsOperationCanceledException()
    {
        var exception = new OperationCanceledException();

        var sut = JobSchedulerServiceExceptionMapper.MapToSchedulerServiceException(exception);

        Assert.That(sut.SourceException, Is.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void GetReason_ReturnsEntityNotFoundForEntityNotFoundException()
    {
        var exception = new EntityNotFoundException(typeof(Domain.Jobs.Job), Guid.NewGuid());

        var result = JobSchedulerServiceExceptionMapper.MapToSchedulerServiceException(exception);
        
        Assert.That(result.SourceException, Is.TypeOf<JobSchedulerServiceException>());
        Assert.That((result.SourceException as JobSchedulerServiceException)!.Reason, Is.EqualTo(JobSchedulerServiceExceptionReason.EntityNotFound));
    }

    [Test]
    public void GetReason_ReturnsEntityInvalidStateTransitionForStateTransitionException()
    {
        var exception = new StateTransitionException("InProgress", "InProgress");

        var result = JobSchedulerServiceExceptionMapper.MapToSchedulerServiceException(exception);

        Assert.That(result.SourceException, Is.TypeOf<JobSchedulerServiceException>());
        Assert.That((result.SourceException as JobSchedulerServiceException)!.Reason, Is.EqualTo(JobSchedulerServiceExceptionReason.EntityInvalidStateTransition));
    }

    [Test]
    public void GetReason_ReturnsServiceCommunicationProblemForTransientPersistenceFaultException()
    {
        var exception = new TransientPersistenceFaultException(new Exception());

        var result = JobSchedulerServiceExceptionMapper.MapToSchedulerServiceException(exception);
        
        Assert.That(result.SourceException, Is.TypeOf<JobSchedulerServiceException>());
        Assert.That((result.SourceException as JobSchedulerServiceException)!.Reason, Is.EqualTo(JobSchedulerServiceExceptionReason.ServiceCommunicationProblem));
    }

    [Test]
    public void GetReason_ReturnsInternalErrorForUnknownException()
    {
        var exception = new Exception();

        var result = JobSchedulerServiceExceptionMapper.MapToSchedulerServiceException(exception);

        Assert.That(result.SourceException, Is.TypeOf<JobSchedulerServiceException>());
        Assert.That((result.SourceException as JobSchedulerServiceException)!.Reason, Is.EqualTo(JobSchedulerServiceExceptionReason.InternalError));
    }
}
