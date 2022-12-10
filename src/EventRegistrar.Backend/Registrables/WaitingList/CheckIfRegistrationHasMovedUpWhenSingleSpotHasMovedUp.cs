using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.Price;

namespace EventRegistrar.Backend.Registrables.WaitingList;

public class CheckIfRegistrationHasMovedUpWhenSingleSpotHasMovedUp : IEventToCommandTranslation<SingleSpotMovedUpFromWaitingList>
{
    public IEnumerable<IRequest> Translate(SingleSpotMovedUpFromWaitingList e)
    {
        yield return new RecalculatePriceAndWaitingListCommand { RegistrationId = e.RegistrationId };
    }
}