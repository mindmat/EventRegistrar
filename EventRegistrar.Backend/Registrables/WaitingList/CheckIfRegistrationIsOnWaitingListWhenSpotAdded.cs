using System.Collections.Generic;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrables.WaitingList
{
    public class CheckIfRegistrationIsOnWaitingListWhenSpotAdded : IEventToCommandTranslation<SpotAdded>
    {
        public IEnumerable<IQueueBoundMessage> Translate(SpotAdded e)
        {
            if (!e.IsInitialProcessing)
            {
                yield return new CheckIfRegistrationIsPromotedCommand { RegistrationId = e.RegistrationId };
            }
        }
    }
}