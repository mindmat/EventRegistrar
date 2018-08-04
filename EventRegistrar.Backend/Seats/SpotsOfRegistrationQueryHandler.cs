using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Registrations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Seats
{
    public class SpotsOfRegistrationQueryHandler : IRequestHandler<SpotsOfRegistrationQuery, IEnumerable<Spot>>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly IQueryable<Registration> _registrations;
        private readonly IQueryable<Seat> _seats;

        public SpotsOfRegistrationQueryHandler(IQueryable<Seat> seats,
                                               IQueryable<Registration> registrations,
                                               IEventAcronymResolver acronymResolver)
        {
            _seats = seats;
            _registrations = registrations;
            _acronymResolver = acronymResolver;
        }

        public async Task<IEnumerable<Spot>> Handle(SpotsOfRegistrationQuery query, CancellationToken cancellationToken)
        {
            var eventId = await _acronymResolver.GetEventIdFromAcronym(query.EventAcronym);
            var spots = await _seats.Where(seat => seat.Registration.EventId == eventId
                                                && (seat.RegistrationId == query.RegistrationId
                                                 || seat.RegistrationId_Follower == query.RegistrationId))
                                    .Where(seat => !seat.IsCancelled)
                                    .Select(seat => new Spot
                                    {
                                        Id = seat.Id,
                                        RegistrableId = seat.RegistrableId,
                                        Registrable = seat.Registrable.Name,
                                        SortKey = seat.Registrable.ShowInMailListOrder,
                                        PartnerRegistrationId = seat.PartnerEmail != null ?
                                            seat.RegistrationId == query.RegistrationId ?
                                                seat.RegistrationId_Follower :
                                                seat.RegistrationId :
                                            null,
                                        FirstPartnerJoined = seat.FirstPartnerJoined,
                                        IsCore = seat.Registrable.IsCore
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