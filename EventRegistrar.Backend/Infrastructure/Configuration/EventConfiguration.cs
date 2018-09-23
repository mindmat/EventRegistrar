using System;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.Infrastructure.Configuration
{
    public class EventConfiguration : Entity
    {
        public Event Event { get; set; }
        public Guid EventId { get; set; }
        public string Type { get; set; }
        public string ValueJson { get; set; }
    }
}