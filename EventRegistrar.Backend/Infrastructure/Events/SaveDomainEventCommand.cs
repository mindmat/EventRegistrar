using System;
using EventRegistrar.Backend.Infrastructure.ServiceBus;

namespace EventRegistrar.Backend.Infrastructure.Events
{
    public class SaveDomainEventCommand : IQueueBoundMessage
    {
        public string EventData { get; set; }
        public Guid? EventId { get; set; }
        public string EventType { get; set; }
        public string QueueName => "SaveDomainEventQueue";
    }
}