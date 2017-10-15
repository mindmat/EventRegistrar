using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventRegistrator.Functions.Infrastructure.DomainEvents
{
    public class DomainEvent
    {
        public Guid Id { get; set; }
        public Guid? AggregateId { get; set; }
        public DateTime Timestamp { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Sequence { get; set; }

        public string Type { get; set; }
        public string Data { get; set; }
    }
}