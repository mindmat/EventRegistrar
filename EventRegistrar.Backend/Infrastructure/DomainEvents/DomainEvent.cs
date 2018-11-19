using System;

namespace EventRegistrar.Backend.Infrastructure.DomainEvents
{
    public class DomainEvent
    {
        public Guid? DomainEventId_Parent { get; set; }
        public Guid? EventId { get; set; }
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
    }
}