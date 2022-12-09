using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrables.WaitingList;

public class CheckIfRegistrationHasMovedUpWhenPartnerSpotHasMovedUp : IEventToCommandTranslation<PartnerSpotMovedUpFromWaitingList>
{
    public IEnumerable<IRequest> Translate(PartnerSpotMovedUpFromWaitingList e)
    {
        if (e.RegistrationId != null)
        {
            yield return new CheckIfRegistrationHasMovedUpCommand { RegistrationId = e.RegistrationId.Value };
        }

        if (e.RegistrationId_Follower != null)
        {
            yield return new CheckIfRegistrationHasMovedUpCommand { RegistrationId = e.RegistrationId_Follower.Value };
        }
    }
}