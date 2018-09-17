using System.Linq;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using SimpleInjector;

namespace EventRegistrar.Backend.Infrastructure.Events
{
    public class EventBus
    {
        //private readonly IEnumerable<IEventToCommandTranslation> _eventToCommandTranslations;
        private readonly Container _container;

        private readonly ServiceBusClient _serviceBusClient;

        public EventBus(Container container,
                        ServiceBusClient serviceBusClient)
        {
            //_eventToCommandTranslations = eventToCommandTranslations;
            _container = container;
            _serviceBusClient = serviceBusClient;
        }

        public void Publish<TEvent>(TEvent @event)
           where TEvent : Event
        {
            var translations = _container.GetAllInstances<IEventToCommandTranslation<TEvent>>().ToList();
            //var translations = _eventToCommandTranslations.Where(tlt => tlt.EventType == typeof(TEvent));
            foreach (var translation in translations)
            {
                var command = translation.Translate(@event);
                _serviceBusClient.SendMessage(command);
            }
        }
    }
}