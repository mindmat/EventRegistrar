using EventRegistrar.Backend.Infrastructure.Events;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations.Price
{
    public class RecalculatePriceWhenSpotAdded : IEventToCommandTranslation<SpotAdded>
    {
        public IQueueBoundMessage Translate(SpotAdded e)
        {
            return new RecalculatePriceCommand { RegistrationId = e.RegistrationId };
        }
    }
}