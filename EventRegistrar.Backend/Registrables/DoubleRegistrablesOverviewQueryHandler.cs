using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrables
{
    public class DoubleRegistrablesOverviewQueryHandler : IRequestHandler<DoubleRegistrablesOverviewQuery, IEnumerable<DoubleRegistrableDisplayItem>>
    {
        private readonly IQueryable<Registrable> _registrables;

        public DoubleRegistrablesOverviewQueryHandler(IQueryable<Registrable> registrables)
        {
            _registrables = registrables;
        }

        public async Task<IEnumerable<DoubleRegistrableDisplayItem>> Handle(DoubleRegistrablesOverviewQuery query, CancellationToken cancellationToken)
        {
            var registrables = await _registrables.Where(rbl => rbl.EventId == query.EventId
                                                             && rbl.MaximumDoubleSeats.HasValue)
                                                  .OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                                                  .Include(rbl => rbl.Seats)
                                                  .ToListAsync(cancellationToken);

            return registrables.Select(rbl => new DoubleRegistrableDisplayItem
            {
                Id = rbl.Id,
                Name = rbl.Name,
                SpotsAvailable = rbl.MaximumDoubleSeats,
                MaximumAllowedImbalance = rbl.MaximumAllowedImbalance,
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