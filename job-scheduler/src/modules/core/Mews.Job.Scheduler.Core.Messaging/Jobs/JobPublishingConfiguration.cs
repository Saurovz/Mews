namespace Mews.Job.Scheduler.Core.Messaging.Jobs;

public sealed class JobPublishingConfiguration
{
    public static readonly string SectionName = "Messaging:ServiceBus:Connections:Scheduler:Publishing";

    public readonly string ConnectionName = "Scheduler";
    
    public required string TopicName { get; set; }
}
