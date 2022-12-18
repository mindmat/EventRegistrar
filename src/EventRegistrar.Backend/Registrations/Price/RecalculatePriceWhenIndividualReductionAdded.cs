using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.IndividualReductions;

namespace EventRegistrar.Backend.Registrations.Price;

public class RecalculatePriceWhenIndividualReductionChanged : IEventToCommandTranslation<IndividualReductionAdded>,
                                                              IEventToCommandTranslation<IndividualReductionRemoved>
{
    public IEnumerable<IRequest> Translate(IndividualReductionAdded e)
    {
        yield return new RecalculatePriceAndWaitingListCommand { RegistrationId = e.RegistrationId };
    }

    public IEnumerable<IRequest> Translate(IndividualReductionRemoved e)
    {
        yield return new RecalculatePriceAndWaitingListCommand { RegistrationId = e.RegistrationId };
    }
}