using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.Price;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrables.WaitingList;

public class CheckIfRegistrationIsOnWaitingListWhenSpotAdded : IEventToCommandTranslation<SpotAdded>
{
    public IEnumerable<IRequest> Translate(SpotAdded e)
    {
        if (!e.IsInitialProcessing)
        {
            yield return new RecalculatePriceAndWaitingListCommand { RegistrationId = e.RegistrationId };
        }
    }
}