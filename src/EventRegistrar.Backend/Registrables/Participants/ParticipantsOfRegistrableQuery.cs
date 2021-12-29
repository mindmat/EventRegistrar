using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Spots;

using MediatR;

namespace EventRegistrar.Backend.Registrables.Participants;

public class ParticipantsOfRegistrableQuery : IRequest<RegistrableDisplayInfo>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrableId { get; set; }
}

public class ParticipantsOfRegistrableQueryHandler : IRequestHandler<ParticipantsOfRegistrableQuery, RegistrableDisplayInfo>
{
    private readonly IQueryable<Registrable> _registrables;
    private readonly IQueryable<Seat> _seats;

    public ParticipantsOfRegistrableQueryHandler(IQueryable<Registrable> registrables,
                                                 IQueryable<Seat> seats)
    {
        _registrables = registrables;
        _seats = seats;
    }

    public async Task<RegistrableDisplayInfo> Handle(ParticipantsOfRegistrableQuery query,
                                                     CancellationToken cancellationToken)
    {
        var registrable = await _registrables.FirstOrDefaultAsync(rbl => rbl.Id == query.RegistrableId, cancellationToken);
        if (registrable == null)
        {
            throw new ArgumentOutOfRangeException($"No registrable found with id {query.RegistrableId}");
        }

        if (registrable.EventId != query.EventId)
        {
            throw new ArgumentException($"Registrable {registrable.Name}  ({registrable.Id}) is not part of requested event {query.EventId}");
        }

        var participants = await _seats.Where(spot => spot.RegistrableId == query.RegistrableId
                                                   && !spot.IsCancelled)
                                       .OrderBy(spot => spot.IsWaitingList)
                                       .ThenBy(spot => spot.FirstPartnerJoined)
                                       .Select(seat => new SpotDisplayInfo
                                                       {
                                                           Leader = seat.RegistrationId == null
                                                               ? null
                                                               : new RegistrationDisplayInfo
                                                                 {
                                                                     Id = seat.Registration!.Id,
                                                                     Email = seat.Registration.RespondentEmail,
                                                                     State = seat.Registration.State,
                                                                     FirstName = seat.Registration.RespondentFirstName,
                                                                     LastName = seat.Registration.RespondentLastName
                                                                 },
                                                           Follower = seat.RegistrationId_Follower == null
                                                               ? null
                                                               : new RegistrationDisplayInfo
                                                                 {
                                                                     Id = seat.Registration_Follower!.Id,
                                                                     Email = seat.Registration_Follower.RespondentEmail,
                                                                     State = seat.Registration_Follower.State,
                                                                     FirstName = seat.Registration_Follower
                                                                                     .RespondentFirstName,
                                                                     LastName = seat.Registration_Follower
                                                                                    .RespondentLastName
                                                                 },
                                                           PlaceholderPartner = seat.IsPartnerSpot &&
                                                                                (seat.RegistrationId == null ||
                                                                                 seat.RegistrationId_Follower == null)
                                                               ? seat.PartnerEmail
                                                               : null,
                                                           IsOnWaitingList = seat.IsWaitingList ||
                                                                             seat.Registration != null &&
                                                                             seat.Registration.IsWaitingList == true,
                                                           IsPartnerRegistration = seat.IsPartnerSpot ||
                                                                                   seat.PartnerEmail != null
                                                       })
                                       .ToListAsync(cancellationToken);

        return new RegistrableDisplayInfo
               {
                   Name = registrable.Name,
                   NameSecondary = registrable.NameSecondary,
                   MaximumDoubleSeats = registrable.MaximumDoubleSeats,
                   MaximumSingleSeats = registrable.MaximumSingleSeats,
                   MaximumAllowedImbalance = registrable.MaximumAllowedImbalance,
                   HasWaitingList = registrable.HasWaitingList,
                   AutomaticPromotionFromWaitingList = registrable.AutomaticPromotionFromWaitingList,
                   Participants = participants.Where(prt => !prt.IsOnWaitingList),
                   WaitingList = participants.Where(prt => prt.IsOnWaitingList)
               };
    }
}