using System;
using System.Linq;
using EventRegistrar.Backend.Events.Context;
using EventRegistrar.Backend.Events.UsersInEvents;
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
        private readonly AuthenticatedUserId _user;

        public EventBus(Container container,
                        ServiceBusClient serviceBusClient,
                        EventContext eventContext,
                        AuthenticatedUserId user)
        {
            _container = container;
            _serviceBusClient = serviceBusClient;
            _eventContext = eventContext;
            _user = user;
        }

        public void Publish<TEvent>(TEvent @event)
            where TEvent : DomainEvent
        {
            if (@event.Id == Guid.Empty)
            {
                @event.Id = Guid.NewGuid();
            }

            if (@event.UserId == null)
            {
                @event.UserId = _user.UserId;
            }

            _serviceBusClient.SendMessage(new SaveDomainEventCommand
            {
                DomainEventId = @event.Id,
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