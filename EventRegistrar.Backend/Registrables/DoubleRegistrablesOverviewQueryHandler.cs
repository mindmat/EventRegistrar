using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Registrations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrables
{
    public class DoubleRegistrablesOverviewQueryHandler : IRequestHandler<DoubleRegistrablesOverviewQuery, IEnumerable<DoubleRegistrableDisplayItem>>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly IQueryable<Registrable> _registrables;

        public DoubleRegistrablesOverviewQueryHandler(IQueryable<Registrable> registrables,
                                                      IQueryable<Registration> registrations,
                                                      IEventAcronymResolver acronymResolver)
        {
            _registrables = registrables;
            _acronymResolver = acronymResolver;
        }

        public async Task<IEnumerable<DoubleRegistrableDisplayItem>> Handle(DoubleRegistrablesOverviewQuery request, CancellationToken cancellationToken)
        {
            var eventId = await _acronymResolver.GetEventIdFromAcronym(request.EventAcronym);
            var registrables = await _registrables.Where(rbl => rbl.EventId == eventId
                                                             && rbl.MaximumDoubleSeats.HasValue)
                                                  .OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                                                  .Include(rbl => rbl.Seats)
                                                  .ToListAsync(cancellationToken);

            return registrables.Select(rbl => new DoubleRegistrableDisplayItem
            {
                Id = rbl.Id,
                Name = rbl.Name,
                SpotsAvailable = rbl.MaximumDoubleSeats,
                LeadersAccepted = rbl.Seats.Count(seat => !seat.IsCancelled &&
                                                          seat.RegistrationId.HasValue &&
                                                          !seat.IsWaitingList),
                FollowersAccepted = rbl.Seats.Count(seat => !seat.IsCancelled &&
                                                            seat.RegistrationId_Follower.HasValue &&
                                                            !seat.IsWaitingList),
                LeadersOnWaitingList = rbl.Seats.Count(seat => !seat.IsCancelled &&
                                                               seat.RegistrationId.HasValue &&
                                                               seat.IsWaitingList &&
                                                               seat.PartnerEmail == null),
                FollowersOnWaitingList = rbl.Seats.Count(seat => !seat.IsCancelled &&
                                                                 seat.RegistrationId_Follower.HasValue &&
                                                                 seat.IsWaitingList &&
                                                                 seat.PartnerEmail == null),
                CouplesOnWaitingList = rbl.Seats.Count(seat => !seat.IsCancelled &&
                                                               seat.RegistrationId_Follower.HasValue &&
                                                               seat.IsWaitingList &&
                                                               seat.PartnerEmail != null)
            });
        }
    }
}