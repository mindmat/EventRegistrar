﻿using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrables.Participants;

public class ParticipantsOfRegistrableQuery : IRequest<RegistrableDisplayInfo>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrableId { get; set; }
}

public class ParticipantsOfRegistrableQueryHandler(IQueryable<Registrable> registrables,
                                                   IQueryable<Seat> seats)
    : IRequestHandler<ParticipantsOfRegistrableQuery, RegistrableDisplayInfo>
{
    public async Task<RegistrableDisplayInfo> Handle(ParticipantsOfRegistrableQuery query,
                                                     CancellationToken cancellationToken)
    {
        var registrable = await registrables.FirstOrDefaultAsync(rbl => rbl.Id == query.RegistrableId, cancellationToken);
        if (registrable == null)
        {
            throw new ArgumentOutOfRangeException($"No registrable found with id {query.RegistrableId}");
        }

        if (registrable.EventId != query.EventId)
        {
            throw new ArgumentException($"Registrable {registrable.DisplayName}  ({registrable.Id}) is not part of requested event {query.EventId}");
        }

        var participants = await seats.Where(spot => spot.RegistrableId == query.RegistrableId
                                                  && !spot.IsCancelled)
                                      .OrderBy(spot => spot.IsWaitingList)
                                      .ThenBy(spot => spot.FirstPartnerJoined)
                                      .Select(spot => new SpotDisplayInfo
                                                      {
                                                          Leader = spot.RegistrationId == null
                                                                       ? null
                                                                       : new RegistrationDisplayInfo
                                                                         {
                                                                             Id = spot.Registration!.Id,
                                                                             Email = spot.Registration.RespondentEmail,
                                                                             State = spot.Registration.State,
                                                                             FirstName = spot.Registration.RespondentFirstName,
                                                                             LastName = spot.Registration.RespondentLastName,
                                                                             Switched = spot.Registrable!.Type == RegistrableType.Double
                                                                                     && spot.Registration.DefaultRole == Role.Follower,
                                                                             CanSwitch = spot.Registrable!.Type == RegistrableType.Double
                                                                                      && spot.Registration.CanSwitchRole
                                                                                      && spot.Registration.DefaultRole == Role.Leader
                                                                         },
                                                          Follower = spot.RegistrationId_Follower == null
                                                                         ? null
                                                                         : new RegistrationDisplayInfo
                                                                           {
                                                                               Id = spot.Registration_Follower!.Id,
                                                                               Email = spot.Registration_Follower.RespondentEmail,
                                                                               State = spot.Registration_Follower.State,
                                                                               FirstName = spot.Registration_Follower.RespondentFirstName,
                                                                               LastName = spot.Registration_Follower.RespondentLastName,
                                                                               Switched = spot.Registration_Follower.DefaultRole == Role.Leader,
                                                                               CanSwitch = spot.Registration_Follower.CanSwitchRole
                                                                                        && spot.Registration_Follower.DefaultRole == Role.Follower
                                                                           },
                                                          PlaceholderPartner = spot.IsPartnerSpot && (spot.RegistrationId == null || spot.RegistrationId_Follower == null)
                                                                                   ? spot.PartnerEmail
                                                                                   : null,
                                                          IsOnWaitingList = spot.IsWaitingList || (spot.Registration != null && spot.Registration.IsOnWaitingList == true),
                                                          IsPartnerRegistration = spot.IsPartnerSpot || spot.PartnerEmail != null
                                                      })
                                      .ToListAsync(cancellationToken);

        var result = new RegistrableDisplayInfo
                     {
                         Id = registrable.Id,
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
        result.AcceptedLeaders = result.Participants.Count(spt => spt.Leader != null);
        result.AcceptedFollowers = result.Participants.Count(spt => spt.Follower != null);
        result.LeadersOnWaitingList = result.WaitingList.Count(spt => spt.Leader != null);
        result.FollowersOnWaitingList = result.WaitingList.Count(spt => spt.Follower != null);
        return result;
    }
}