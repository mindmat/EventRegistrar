using EventRegistrator.Functions.Infrastructure.DataAccess;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace EventRegistrator.Functions.Infrastructure.DomainEvents
{
    public class DomainEventPersistor
    {
        public static async Task Log<T>(T @event, Guid? aggregateId = null)
        {
            using (var context = new EventRegistratorDbContext())
            {
                context.DomainEvents.Add(new DomainEvent
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.UtcNow,
                    Type = @event.GetType().FullName,
                    Data = JsonConvert.SerializeObject(@event),
                    AggregateId = aggregateId
                });
                await context.SaveChangesAsync();
            }
        }
    }
}