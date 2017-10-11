using Newtonsoft.Json;
using System;
using System.Data.Entity;

namespace EventRegistrator.Functions.Infrastructure.DomainEvents
{
    public static class DomainEventsExtensionMethods
    {
        public static void Save<T>(this DbSet<DomainEvent> domainEvents, T @event, Guid aggregateId)
        {
            domainEvents.Add(new DomainEvent
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Type = @event.GetType().FullName,
                Data = JsonConvert.SerializeObject(@event),
                AggregateId = aggregateId
            });
        }
    }
}