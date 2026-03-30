using Mews.Atlas.Messaging;
using Mews.Atlas.Messaging.ServiceBus;
using Microsoft.Extensions.Options;

namespace Mews.Job.Scheduler.Core.Messaging.Jobs;

public sealed class JobPublisher : IJobPublisher
{
    private readonly IMessagePublisher _messagePublisher;
    private readonly JobPublishingConfiguration _publishingConfiguration;

    public JobPublisher(
        IMessagePublisher messagePublisher,
        IOptions<JobPublishingConfiguration> publishingConfiguration)
    {
        _messagePublisher = messagePublisher;
        _publishingConfiguration = publishingConfiguration.Value;
    }

    public async Task PublishAsync(List<JobQueueMessage> messageBatch, CancellationToken cancellationToken)
    {
        var messages = messageBatch.Select(m =>
        {
            var message = new MessageEnvelope<JobQueueMessage>(m, m.Identifier, m.Identifier);

            message.AddMetadata(nameof(m.Team), m.Team);
            message.AddMetadata(nameof(m.ExecutorType), m.ExecutorType);
            
            return message;
        }).ToList();

        await _messagePublisher.PublishBatchAsync(messages, GetConfiguration(), cancellationToken);
    }

    private ServiceBusPublishConfiguration GetConfiguration()
    {
        return new ServiceBusPublishConfiguration(_publishingConfiguration.ConnectionName, _publishingConfiguration.TopicName);
    }
}
