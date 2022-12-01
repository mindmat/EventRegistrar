using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrables.WaitingList;

public class CheckIfRegistrationIsPromotedWhenPartnerSpotIsPromoted : IEventToCommandTranslation<PartnerSpotPromotedFromWaitingList>
{
    public IEnumerable<IRequest> Translate(PartnerSpotPromotedFromWaitingList e)
    {
        if (e.RegistrationId != null)
        {
            yield return new CheckIfRegistrationIsPromotedCommand { RegistrationId = e.RegistrationId.Value };
        }

        if (e.RegistrationId_Follower != null)
        {
            yield return new CheckIfRegistrationIsPromotedCommand { RegistrationId = e.RegistrationId_Follower.Value };
        }
    }
}