namespace Mews.Job.Scheduler.Core.Messaging.Jobs;

public interface IJobPublisher
{
    Task PublishAsync(List<JobQueueMessage> messageBatch, CancellationToken cancellationToken);
}
