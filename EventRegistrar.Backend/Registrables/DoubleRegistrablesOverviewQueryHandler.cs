using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Spots;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrables
{
    public class DoubleRegistrablesOverviewQueryHandler : IRequestHandler<DoubleRegistrablesOverviewQuery, IEnumerable<DoubleRegistrableDisplayItem>>
    {
        private readonly IQueryable<Registrable> _registrables;
        private readonly IAuthorizationChecker _authorizationChecker;

        public DoubleRegistrablesOverviewQueryHandler(IQueryable<Registrable> registrables,
                                                      IAuthorizationChecker authorizationChecker)
        {
            _registrables = registrables;
            _authorizationChecker = authorizationChecker;
        }

        public async Task<IEnumerable<DoubleRegistrableDisplayItem>> Handle(DoubleRegistrablesOverviewQuery query, CancellationToken cancellationToken)
        {
            var registrables = await _registrables.Where(rbl => rbl.EventId == query.EventId
                                                             && rbl.MaximumDoubleSeats.HasValue)
                                                  .OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                                                  .Include(rbl => rbl.Spots)
                                                  .ToListAsync(cancellationToken);

            var userCanDeleteRegistrable = await _authorizationChecker.UserHasRight(query.EventId, nameof(DeleteRegistrableCommand));
            return registrables.Select(rbl => new DoubleRegistrableDisplayItem
            {
                Id = rbl.Id,
                Name = rbl.Name,
                SpotsAvailable = rbl.MaximumDoubleSeats,
                AutomaticPromotionFromWaitingList = rbl.AutomaticPromotionFromWaitingList,
                MaximumAllowedImbalance = rbl.MaximumAllowedImbalance,
                LeadersAccepted = rbl.Spots.Count(spt => !spt.IsCancelled
                                                      && !spt.IsWaitingList
                                                      && spt.RegistrationId != null),
                FollowersAccepted = rbl.Spots.Count(spt => !spt.IsCancelled
                                                        && !spt.IsWaitingList
                                                        && spt.RegistrationId_Follower != null),
                LeadersOnWaitingList = rbl.Spots.Count(spt => !spt.IsCancelled
                                                           && spt.IsWaitingList
                                                           && spt.IsSingleLeaderSpot()),
                FollowersOnWaitingList = rbl.Spots.Count(spt => !spt.IsCancelled
                                                             && spt.IsWaitingList
                                                             && spt.IsSingleFollowerSpot()),
                CouplesOnWaitingList = rbl.Spots.Count(spt => !spt.IsCancelled
                                                           && spt.IsWaitingList
                                                           && (spt.IsUnmatchedPartnerSpot() || spt.IsMatchedPartnerSpot())),
                IsDeletable = !rbl.Spots.Any(spt => !spt.IsCancelled)
                                              && userCanDeleteRegistrable
                                              && rbl.Event.State == RegistrationForms.State.Setup
            });
        }
    }
}