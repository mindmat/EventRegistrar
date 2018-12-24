using System.Collections.Generic;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Spots;
using MediatR;

namespace EventRegistrar.Backend.Registrations.Price
{
    public class RecalculatePriceWhenSpotRemoved : IEventToCommandTranslation<SpotRemoved>
    {
        public IEnumerable<IRequest> Translate(SpotRemoved e)
        {
            if (e.Reason == RemoveSpotReason.Modification)
            {
                yield return new RecalculatePriceCommand { RegistrationId = e.RegistrationId };
            }
        }
    }
}