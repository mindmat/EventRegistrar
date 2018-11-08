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
           where TEvent : Event
        {
            _serviceBusClient.SendMessage(new SaveDomainEventCommand
            {
                EventId = _eventContext.EventId,
                EventType = @event.GetType().FullName,
                EventData = JsonConvert.SerializeObject(@event),
            });
            var translations = _container.GetAllInstances<IEventToCommandTranslation<TEvent>>().ToList();
            foreach (var translation in translations)
            {
                var command = translation.Translate(@event);
                _serviceBusClient.SendMessage(command);
            }
        }
    }
}