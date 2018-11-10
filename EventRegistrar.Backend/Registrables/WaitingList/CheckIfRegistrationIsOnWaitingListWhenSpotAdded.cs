using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrables.WaitingList
{
    public class CheckIfRegistrationIsOnWaitingListWhenSpotAdded : IEventToCommandTranslation<SpotAdded>
    {
        public IQueueBoundMessage Translate(SpotAdded e)
        {
            if (!e.IsInitialProcessing)
            {
                return new CheckIfRegistrationIsPromotedCommand { RegistrationId = e.RegistrationId };
            }

            return null;
        }
    }
}