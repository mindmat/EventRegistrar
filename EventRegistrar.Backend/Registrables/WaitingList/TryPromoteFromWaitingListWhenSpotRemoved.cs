using System.Collections.Generic;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrables.WaitingList
{
    public class TryPromoteFromWaitingListWhenSpotRemoved : IEventToCommandTranslation<SpotRemoved>
    {
        public IEnumerable<IQueueBoundMessage> Translate(SpotRemoved e)
        {
            if (e.WasSpotOnWaitingList && e.Reason == RemoveSpotReason.Modification)
            {
                yield return new TryPromoteFromWaitingListCommand { RegistrableId = e.RegistrableId };
            }
        }
    }
}