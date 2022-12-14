using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations.Price;

public class RecalculatePriceWhenSpotChanged : IEventToCommandTranslation<SpotRemoved>, IEventToCommandTranslation<SpotAdded>
{
    public IEnumerable<IRequest> Translate(SpotAdded e)
    {
        if (!e.IsInitialProcessing)
        {
            yield return new RecalculatePriceAndWaitingListCommand { RegistrationId = e.RegistrationId };
        }
    }

    public IEnumerable<IRequest> Translate(SpotRemoved e)
    {
        if (e.Reason == RemoveSpotReason.Modification)
        {
            yield return new RecalculatePriceAndWaitingListCommand { RegistrationId = e.RegistrationId };
        }
    }
}