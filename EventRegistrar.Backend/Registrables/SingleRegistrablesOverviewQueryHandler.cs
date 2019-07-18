﻿using System;
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
                                                  .Include(rbl => rbl.Seats)
                                                  .ToListAsync(cancellationToken);

            var registrationsOnWaitingList = new HashSet<Guid>(_registrations.Where(reg => reg.EventId == query.EventId
                                                                                        && (reg.IsWaitingList ?? false))
                                                                             .Select(reg => reg.Id));

            return registrables.OrderBy(rbl => rbl.ShowInMailListOrder ?? int.MaxValue)
                               .Select(rbl => new SingleRegistrableDisplayItem
                               {
                                   Id = rbl.Id,
                                   Name = rbl.Name,
                                   SpotsAvailable = rbl.MaximumSingleSeats,
                                   Accepted = rbl.Seats.Count(spt => !spt.IsCancelled
                                                                  && !spt.IsWaitingList
                                                                  && !registrationsOnWaitingList.Contains(spt.RegistrationId ?? Guid.Empty)),
                                   OnWaitingList = rbl.Seats.Count(spt => !spt.IsCancelled
                                                                       && (spt.IsWaitingList || registrationsOnWaitingList.Contains(spt.RegistrationId ?? Guid.Empty))),
                                   IsDeletable = !rbl.Seats.Any(spt => !spt.IsCancelled) && _authorizationChecker.UserHasRight(query.EventId, nameof(DeleteRegistrableCommand)).Result
                               });
        }
    }
}