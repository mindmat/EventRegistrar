﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Registrations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Spots
{
    public class SpotsOfRegistrationQueryHandler : IRequestHandler<SpotsOfRegistrationQuery, IEnumerable<Spot>>
    {
        private readonly IQueryable<Registration> _registrations;
        private readonly IQueryable<Seat> _seats;

        public SpotsOfRegistrationQueryHandler(IQueryable<Seat> seats,
                                               IQueryable<Registration> registrations)
        {
            _seats = seats;
            _registrations = registrations;
        }

        public async Task<IEnumerable<Spot>> Handle(SpotsOfRegistrationQuery query, CancellationToken cancellationToken)
        {
            var spots = await _seats.Where(seat => (seat.Registration.EventId == query.EventId ||
                                                    seat.Registration_Follower.EventId == query.EventId)
                                                && (seat.RegistrationId == query.RegistrationId
                                                 || seat.RegistrationId_Follower == query.RegistrationId))
                                    .Where(seat => !seat.IsCancelled)
                                    .Select(seat => new Spot
                                    {
                                        Id = seat.Id,
                                        RegistrableId = seat.RegistrableId,
                                        Registrable = seat.Registrable.Name,
                                        SortKey = seat.Registrable.ShowInMailListOrder,
                                        PartnerRegistrationId = seat.IsPartnerSpot ?
                                            seat.RegistrationId == query.RegistrationId ?
                                                seat.RegistrationId_Follower :
                                                seat.RegistrationId :
                                            null,
                                        FirstPartnerJoined = seat.FirstPartnerJoined,
                                        IsCore = seat.Registrable.IsCore,
                                        Partner = seat.IsPartnerSpot ? seat.PartnerEmail : null
                                    })
                                    .OrderBy(seat => seat.SortKey)
                                    .ToListAsync(cancellationToken);

            foreach (var spot in spots.Where(spot => spot.PartnerRegistrationId != null))
            {
                var names = await _registrations
                                  .Where(reg => reg.Id == spot.PartnerRegistrationId)
                                  .Select(reg => new { reg.RespondentFirstName, reg.RespondentLastName })
                                  .FirstOrDefaultAsync(cancellationToken);
                spot.Partner = $"{names.RespondentFirstName} {names.RespondentLastName}";
            }

            return spots;
        }
    }
}