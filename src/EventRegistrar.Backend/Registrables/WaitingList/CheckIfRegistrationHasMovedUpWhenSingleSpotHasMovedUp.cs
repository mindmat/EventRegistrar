using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrables.WaitingList;

public class CheckIfRegistrationHasMovedUpWhenSingleSpotHasMovedUp : IEventToCommandTranslation<SingleSpotMovedUpFromWaitingList>
{
    public IEnumerable<IRequest> Translate(SingleSpotMovedUpFromWaitingList e)
    {
        yield return new CheckIfRegistrationHasMovedUpCommand { RegistrationId = e.RegistrationId };
    }
}