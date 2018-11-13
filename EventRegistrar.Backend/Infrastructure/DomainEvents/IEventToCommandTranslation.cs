using System.Collections.Generic;
using EventRegistrar.Backend.Infrastructure.ServiceBus;

namespace EventRegistrar.Backend.Infrastructure.DomainEvents
{
    public interface IEventToCommandTranslation<in TEvent>
        where TEvent : DomainEvent
    {
        IEnumerable<IQueueBoundMessage> Translate(TEvent e);
    }
}