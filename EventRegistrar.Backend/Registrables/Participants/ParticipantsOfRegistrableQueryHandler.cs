using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Seats;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrables.Participants
{
    public class ParticipantsOfRegistrableQueryHandler : IRequestHandler<ParticipantsOfRegistrableQuery, RegistrableDisplayInfo>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly IQueryable<Registrable> _registrables;
        private readonly IQueryable<Seat> _seats;

        public ParticipantsOfRegistrableQueryHandler(IQueryable<Registrable> registrables,
                                                     IQueryable<Seat> seats,
                                                     IEventAcronymResolver acronymResolver)
        {
            _registrables = registrables;
            _seats = seats;
            _acronymResolver = acronymResolver;
        }

        public async Task<RegistrableDisplayInfo> Handle(ParticipantsOfRegistrableQuery query,
                                                         CancellationToken cancellationToken)
        {
            var eventId = await _acronymResolver.GetEventIdFromAcronym(query.EventAcronym);
            var registrable = await _registrables.FirstOrDefaultAsync(rbl => rbl.Id == query.RegistrableId, cancellationToken);
            if (registrable == null)
            {
                throw new ArgumentOutOfRangeException($"No registrable found with id {query.RegistrableId}");
            }
            if (registrable.EventId != eventId)
            {
                throw new ArgumentException($"Registrable {registrable.Name}({registrable.Id}) is not part of requested event {eventId}");
            }
            var participants = await _seats.Where(seat => seat.RegistrableId == query.RegistrableId
                                                       && !seat.IsCancelled)
                                           .OrderBy(seat => seat.IsWaitingList)
                                           .ThenBy(seat => seat.FirstPartnerJoined)
                                           .Select(seat => new PlaceDisplayInfo
                                           {
                                               Leader = seat.RegistrationId == null ? null : new RegistrationDisplayInfo
                                               {
                                                   Id = seat.Registration.Id,
                                                   Email = seat.Registration.RespondentEmail,
                                                   State = seat.Registration.State,
                                                   FirstName = seat.Registration.RespondentFirstName,
                                                   LastName = seat.Registration.RespondentLastName
                                               },
                                               Follower = seat.RegistrationId_Follower == null ? null : new RegistrationDisplayInfo
                                               {
                                                   Id = seat.Registration_Follower.Id,
                                                   Email = seat.Registration_Follower.RespondentEmail,
                                                   State = seat.Registration_Follower.State,
                                                   FirstName = seat.Registration_Follower.RespondentFirstName,
                                                   LastName = seat.Registration_Follower.RespondentLastName
                                               },
                                               IsOnWaitingList = seat.IsWaitingList || seat.Registration != null && seat.Registration.IsWaitingList == true,
                                               IsPartnerRegistration = seat.IsPartnerSpot || seat.PartnerEmail != null
                                           })
                                           .ToListAsync(cancellationToken);

            return new RegistrableDisplayInfo
            {
                Name = registrable.Name,
                MaximumDoubleSeats = registrable.MaximumDoubleSeats,
                MaximumSingleSeats = registrable.MaximumSingleSeats,
                MaximumAllowedImbalance = registrable.MaximumAllowedImbalance,
                HasWaitingList = registrable.HasWaitingList,
                Participants = participants.Where(prt => !prt.IsOnWaitingList),
                WaitingList = participants.Where(prt => prt.IsOnWaitingList)
            };
        }
    }
}