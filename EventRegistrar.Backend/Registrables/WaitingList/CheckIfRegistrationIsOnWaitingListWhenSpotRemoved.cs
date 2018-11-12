using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrables.WaitingList
{
    public class CheckIfRegistrationIsOnWaitingListWhenSpotRemoved : IEventToCommandTranslation<SpotRemoved>
    {
        public IQueueBoundMessage Translate(SpotRemoved e)
        {
            if (e.Reason == RemoveSpotReason.Modification)
            {
                return new CheckIfRegistrationIsPromotedCommand { RegistrationId = e.RegistrationId };
            }

            return null;
        }
    }
}