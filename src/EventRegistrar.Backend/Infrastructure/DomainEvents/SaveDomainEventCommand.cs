using EventRegistrar.Backend.Infrastructure.ServiceBus;

namespace EventRegistrar.Backend.Infrastructure.DomainEvents;

public class SaveDomainEventCommand : IQueueBoundMessage
{
    public Guid DomainEventId { get; set; }
    public Guid? DomainEventId_Parent { get; set; }
    public string EventData { get; set; }
    public Guid? EventId { get; set; }
    public string EventType { get; set; }
    public string QueueName => "SaveDomainEventQueue";
}