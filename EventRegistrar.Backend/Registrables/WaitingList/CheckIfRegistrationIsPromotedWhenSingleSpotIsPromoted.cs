using System.Collections.Generic;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;

namespace EventRegistrar.Backend.Registrables.WaitingList
{
    public class CheckIfRegistrationIsPromotedWhenSingleSpotIsPromoted : IEventToCommandTranslation<SingleSpotPromotedFromWaitingList>
    {
        public IEnumerable<IQueueBoundMessage> Translate(SingleSpotPromotedFromWaitingList e)
        {
            yield return new CheckIfRegistrationIsPromotedCommand { RegistrationId = e.RegistrationId };
        }
    }
}