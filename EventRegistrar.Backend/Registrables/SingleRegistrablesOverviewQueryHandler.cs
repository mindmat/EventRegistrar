using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Registrations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrables
{
    public class SingleRegistrablesOverviewQueryHandler : IRequestHandler<SingleRegistrablesOverviewQuery, IEnumerable<SingleRegistrableDisplayItem>>
    {
        private readonly IQueryable<Registrable> _registrables;
        private readonly IQueryable<Registration> _registrations;
        private readonly IAuthorizationChecker _authorizationChecker;

        public SingleRegistrablesOverviewQueryHandler(IQueryable<Registrable> registrables,
                                                      IQueryable<Registration> registrations,
                                                      IAuthorizationChecker authorizationChecker)
        {
            _registrables = registrables;
            _registrations = registrations;
            _authorizationChecker = authorizationChecker;
        }

        public async Task<IEnumerable<SingleRegistrableDisplayItem>> Handle(SingleRegistrablesOverviewQuery query, CancellationToken cancellationToken)
        {
            var registrables = await _registrables.Where(rbl => rbl.EventId == query.EventId
                                                             && !rbl.MaximumDoubleSeats.HasValue)
                                                  .OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                                                  .Include(rbl => rbl.Spots)
                                                  .ToListAsync(cancellationToken);

            var registrationsOnWaitingList = new HashSet<Guid>(_registrations.Where(reg => reg.EventId == query.EventId
                                                                                        && (reg.IsWaitingList ?? false))
                                                                             .Select(reg => reg.Id));
            var userCanDeleteRegistrable = await _authorizationChecker.UserHasRight(query.EventId, nameof(DeleteRegistrableCommand));
            return registrables.OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                               .Select(rbl => new SingleRegistrableDisplayItem
                               {
                                   Id = rbl.Id,
                                   Name = rbl.Name,
                                   SpotsAvailable = rbl.MaximumSingleSeats,
                                   AutomaticPromotionFromWaitingList = rbl.AutomaticPromotionFromWaitingList,
                                   Accepted = rbl.Spots.Count(spt => !spt.IsCancelled
                                                                  && !spt.IsWaitingList
                                                                  && !registrationsOnWaitingList.Contains(spt.RegistrationId ?? Guid.Empty)),
                                   OnWaitingList = rbl.Spots.Count(spt => !spt.IsCancelled
                                                                       && (spt.IsWaitingList || registrationsOnWaitingList.Contains(spt.RegistrationId ?? Guid.Empty))),
                                   IsDeletable = !rbl.Spots.Any(spt => !spt.IsCancelled)
                                              && userCanDeleteRegistrable
                                              && rbl.Event.State == RegistrationForms.State.Setup
                               });
        }
    }
}