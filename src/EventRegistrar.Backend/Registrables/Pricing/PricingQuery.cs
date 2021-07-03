using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authorization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrables.Pricing
{
    public class PricingQuery : IRequest<IEnumerable<RegistrablePricing>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }

    public class PricingQueryHandler : IRequestHandler<PricingQuery, IEnumerable<RegistrablePricing>>
    {
        private readonly IQueryable<Registrable> _registrables;

        public PricingQueryHandler(IQueryable<Registrable> registrables)
        {
            _registrables = registrables;
        }

        public async Task<IEnumerable<RegistrablePricing>> Handle(PricingQuery query, CancellationToken cancellationToken)
        {
            return await _registrables.Where(rbl => rbl.EventId == query.EventId)
                                      .OrderByDescending(rbl => rbl.Price ?? 0m)
                                      .ThenBy(rbl => rbl.Name)
                                      .Select(rbl => new RegistrablePricing
                                      {
                                          RegistrableId = rbl.Id,
                                          RegistrableName = rbl.Name,
                                          Price = rbl.Price,
                                          ReducedPrice = rbl.ReducedPrice,
                                          Reductions = rbl.Reductions.Where(red => !red.ActivatedByReduction)
                                                                     .Select(red => new PricingReduction
                                                                     {
                                                                         Id = red.Id,
                                                                         Amount = red.Amount,
                                                                         RegistrableId1_ReductionActivatedIfCombinedWith = red.RegistrableId1_ReductionActivatedIfCombinedWith,
                                                                         RegistrableId2_ReductionActivatedIfCombinedWith = red.RegistrableId2_ReductionActivatedIfCombinedWith
                                                                     })
                                      })
                                      .ToListAsync();
        }
    }
}
