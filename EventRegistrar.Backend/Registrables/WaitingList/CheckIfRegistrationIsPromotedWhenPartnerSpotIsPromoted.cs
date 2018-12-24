using System.Collections.Generic;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using MediatR;

namespace EventRegistrar.Backend.Registrables.WaitingList
{
    public class CheckIfRegistrationIsPromotedWhenPartnerSpotIsPromoted : IEventToCommandTranslation<PartnerSpotPromotedFromWaitingList>
    {
        public IEnumerable<IRequest> Translate(PartnerSpotPromotedFromWaitingList e)
        {
            if (e.RegistrationId.HasValue)
            {
                yield return new CheckIfRegistrationIsPromotedCommand { RegistrationId = e.RegistrationId.Value };
            }
            if (e.RegistrationId_Follower.HasValue)
            {
                yield return new CheckIfRegistrationIsPromotedCommand { RegistrationId = e.RegistrationId_Follower.Value };
            }
        }
    }
}