using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrables
{
    public class RegistrablesQueryHandler : IRequestHandler<RegistrablesQuery, IEnumerable<RegistrableItem>>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly IQueryable<Registrable> _registrables;

        public RegistrablesQueryHandler(IQueryable<Registrable> registrables,
                                        IEventAcronymResolver acronymResolver)
        {
            _registrables = registrables;
            _acronymResolver = acronymResolver;
        }

        public async Task<IEnumerable<RegistrableItem>> Handle(RegistrablesQuery query, CancellationToken cancellationToken)
        {
            var eventId = await _acronymResolver.GetEventIdFromAcronym(query.EventAcronym);
            var registrables = await _registrables.Where(rbl => rbl.EventId == eventId)
                                                  .Select(rbl => new RegistrableItem
                                                  {
                                                      Id = rbl.Id,
                                                      Name = rbl.Name,
                                                      HasWaitingList = rbl.HasWaitingList,
                                                      IsDoubleRegistrable = rbl.MaximumDoubleSeats.HasValue,
                                                      ShowInMailListOrder = rbl.ShowInMailListOrder
                                                  })
                                                  .OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                                                  .ToListAsync(cancellationToken);

            return registrables;
        }
    }
}