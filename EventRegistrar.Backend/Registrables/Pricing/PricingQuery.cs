using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Registrables.Pricing
{
    public class PricingQuery : IRequest<IEnumerable<RegistrablePricing>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }
}
