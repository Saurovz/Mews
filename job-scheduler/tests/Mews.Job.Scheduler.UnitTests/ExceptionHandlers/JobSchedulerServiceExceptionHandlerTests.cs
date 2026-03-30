using System.Text.Json;
using Mews.Atlas.Alerting;
using Mews.Job.Scheduler.ExceptionHandlers;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Mews.Job.Scheduler.UnitTests.ExceptionHandlers;

[TestFixture]
public class JobSchedulerServiceExceptionHandlerTests
{
    [TestCase(JobSchedulerServiceExceptionReason.EntityNotFound, 404)]
    [TestCase(JobSchedulerServiceExceptionReason.EntityInvalidStateTransition, 400)]
    [TestCase(JobSchedulerServiceExceptionReason.EntityProcessingConflict, 409)]
    [TestCase(JobSchedulerServiceExceptionReason.ServiceCommunicationProblem, 503)]
    [TestCase(JobSchedulerServiceExceptionReason.InternalError, 500)]
    public async Task TryHandleAsync_ReturnsExpectedStatusCode(JobSchedulerServiceExceptionReason reason,
        int expectedStatusCode)
    {
        var reporter = Substitute.For<IIncidentReporter>();
        var handler = new JobSchedulerServiceExceptionHandler(reporter);
        
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var exception = new JobProcessingException(reason, new Exception("Simulated failure"));

        // Act
        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.That(handled, Is.True);
        Assert.That(context.Response.StatusCode, Is.EqualTo(expectedStatusCode));
        
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var jsonDoc = await JsonDocument.ParseAsync(context.Response.Body);
        var root = jsonDoc.RootElement;
        
        Assert.That(root.GetProperty("title").GetString(), Is.EqualTo(reason.ToString()));
        Assert.That(root.GetProperty("detail").GetString(), Is.Not.Null.And.Not.Empty);
        Assert.That(root.GetProperty("status").GetInt32(), Is.EqualTo(expectedStatusCode));
    }
    
    [Test]
    public async Task TryHandleAsync_WhenExceptionIsNotHandled_ReturnsFalse()
    {
        // Arrange
        var reporter = Substitute.For<IIncidentReporter>();
        var handler = new JobSchedulerServiceExceptionHandler(reporter);
        var context = new DefaultHttpContext();

        // Act
        var handled = await handler.TryHandleAsync(context, new InvalidOperationException(), CancellationToken.None);

        // Assert
        Assert.That(handled, Is.False);
    }
    
    [Test]
    public async Task TryHandleAsync_WhenJobProcessingExceptionIsThrown_IsHandledBySchedulerHandler()
    {
        // Arrange
        var reporter = Substitute.For<IIncidentReporter>();
        var handler = new JobSchedulerServiceExceptionHandler(reporter);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var exception = new JobProcessingException(
            JobSchedulerServiceExceptionReason.EntityNotFound,
            new Exception("Something went wrong")
        );

        // Act
        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.That(handled, Is.True);
        Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var json = await JsonDocument.ParseAsync(context.Response.Body);
        var root = json.RootElement;

        Assert.That(root.GetProperty("title").GetString(), Is.EqualTo("EntityNotFound"));
        Assert.That(root.GetProperty("detail").GetString(), Does.Contain("Something went wrong"));
        Assert.That(root.GetProperty("status").GetInt32(), Is.EqualTo(404));
    }
}
