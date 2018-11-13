using System;
using System.Linq;
using EventRegistrar.Backend.Events.Context;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using Newtonsoft.Json;
using SimpleInjector;

namespace EventRegistrar.Backend.Infrastructure.DomainEvents
{
    public class EventBus
    {
        private readonly Container _container;
        private readonly EventContext _eventContext;
        private readonly ServiceBusClient _serviceBusClient;

        public EventBus(Container container,
                        ServiceBusClient serviceBusClient,
                        EventContext eventContext)
        {
            _container = container;
            _serviceBusClient = serviceBusClient;
            _eventContext = eventContext;
        }

        public void Publish<TEvent>(TEvent @event)
            where TEvent : DomainEvent
        {
            _serviceBusClient.SendMessage(new SaveDomainEventCommand
            {
                DomainEventId = @event.Id == Guid.Empty ? Guid.NewGuid() : @event.Id,
                DomainEventId_Parent = @event.DomainEventId_Parent,
                EventId = @event.EventId ?? _eventContext.EventId,
                EventType = @event.GetType().FullName,
                EventData = JsonConvert.SerializeObject(@event),
            });
            var translations = _container.GetAllInstances<IEventToCommandTranslation<TEvent>>().ToList();
            foreach (var command in translations.SelectMany(trn => trn.Translate(@event)).Where(cmd => cmd != null))
            {
                _serviceBusClient.SendMessage(command);
            }
        }
    }
}