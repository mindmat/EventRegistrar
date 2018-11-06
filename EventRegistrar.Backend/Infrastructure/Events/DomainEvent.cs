using System;

namespace EventRegistrar.Backend.Infrastructure.Events
{
    public class DomainEvent
    {
        public string Data { get; set; }
        public Guid? EventId { get; set; }
        public Guid Id { get; set; }

        //public long Sequence { get; set; }
        public DateTime Timestamp { get; set; }

        public string Type { get; set; }
    }
}