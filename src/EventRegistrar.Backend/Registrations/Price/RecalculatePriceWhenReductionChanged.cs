using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.Reductions;
using MediatR;

namespace EventRegistrar.Backend.Registrations.Price;

public class RecalculatePriceWhenReductionChanged : IEventToCommandTranslation<ReductionChanged>
{
    public IEnumerable<IRequest> Translate(ReductionChanged e)
    {
        yield return new RecalculatePriceCommand { RegistrationId = e.RegistrationId };
    }
}