using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.Price;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrables.WaitingList;

public class CheckIfRegistrationIsOnWaitingListWhenSpotRemoved : IEventToCommandTranslation<SpotRemoved>
{
    public IEnumerable<IRequest> Translate(SpotRemoved e)
    {
        if (e.Reason == RemoveSpotReason.Modification)
        {
            yield return new RecalculatePriceAndWaitingListCommand { RegistrationId = e.RegistrationId };
        }
    }
}