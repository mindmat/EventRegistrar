using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations.Price
{
    public class RecalculatePriceWhenSpotAdded : IEventToCommandTranslation<SpotAdded>
    {
        public IQueueBoundMessage Translate(SpotAdded e)
        {
            if (!e.IsInitialProcessing)
            {
                return new RecalculatePriceCommand { RegistrationId = e.RegistrationId };
            }

            return null;
        }
    }
}