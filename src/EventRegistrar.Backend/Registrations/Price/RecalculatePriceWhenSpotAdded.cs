using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Spots;
using MediatR;

namespace EventRegistrar.Backend.Registrations.Price;

public class RecalculatePriceWhenSpotAdded : IEventToCommandTranslation<SpotAdded>
{
    public IEnumerable<IRequest> Translate(SpotAdded e)
    {
        if (!e.IsInitialProcessing) yield return new RecalculatePriceCommand { RegistrationId = e.RegistrationId };
    }
}