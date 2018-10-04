using EventRegistrar.Backend.Infrastructure.Events;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrables.WaitingList
{
    public class CheckIfRegistrationIsOnWaitingListWhenSpotRemoved : IEventToCommandTranslation<SpotRemoved>
    {
        public IQueueBoundMessage Translate(SpotRemoved e)
        {
            return new CheckIfRegistrationIsPromotedCommand { RegistrationId = e.RegistrationId };
        }
    }
}