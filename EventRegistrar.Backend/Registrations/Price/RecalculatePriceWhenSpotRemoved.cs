using EventRegistrar.Backend.Infrastructure.Events;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations.Price
{
    public class RecalculatePriceWhenSpotRemoved : IEventToCommandTranslation<SpotRemoved>
    {
        public IQueueBoundMessage Translate(SpotRemoved e)
        {
            return new RecalculatePriceCommand { RegistrationId = e.RegistrationId };
        }
    }
}