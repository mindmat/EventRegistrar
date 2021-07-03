using System.Collections.Generic;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.IndividualReductions;
using MediatR;

namespace EventRegistrar.Backend.Registrations.Price
{
    public class RecalculatePriceWhenIndividualReductionAdded : IEventToCommandTranslation<IndividualReductionAdded>
    {
        public IEnumerable<IRequest> Translate(IndividualReductionAdded e)
        {
            yield return new RecalculatePriceCommand { RegistrationId = e.RegistrationId };
        }
    }
}