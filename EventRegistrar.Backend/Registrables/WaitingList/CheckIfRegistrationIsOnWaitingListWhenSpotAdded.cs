using EventRegistrar.Backend.Infrastructure.Events;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrables.WaitingList
{
    public class CheckIfRegistrationIsOnWaitingListWhenSpotAdded : IEventToCommandTranslation<SpotAdded>
    {
        public IQueueBoundMessage Translate(SpotAdded e)
        {
            return new CheckIfRegistrationIsPromotedCommand { RegistrationId = e.RegistrationId };
        }
    }
}