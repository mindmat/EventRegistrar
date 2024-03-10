using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrables.Participants;

public class SwitchRoleOfParticipantCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrableId { get; set; }
    public Guid RegistrationId { get; set; }
    public Role ToRole { get; set; }
}

public class SwitchRoleOfParticipantCommandHandler(IRepository<Seat> spots) : IRequestHandler<SwitchRoleOfParticipantCommand>
{
    public async Task Handle(SwitchRoleOfParticipantCommand command, CancellationToken cancellationToken)
    {
        var matchingSpots = await spots.Where(spt => spt.Registrable!.EventId == command.EventId
                                                  && spt.RegistrableId == command.RegistrableId
                                                  && !spt.IsCancelled
                                                  && !spt.IsPartnerSpot)
                                       .WhereIf(command.ToRole == Role.Follower, spt => spt.RegistrationId == command.RegistrationId)
                                       .WhereIf(command.ToRole == Role.Leader, spt => spt.RegistrationId_Follower == command.RegistrationId)
                                       .Include(spt => spt.Registrable)
                                       .Include(spt => spt.Registration)
                                       .Include(spt => spt.Registration_Follower)
                                       .ToListAsync(cancellationToken);
        if (matchingSpots.Count != 1)
        {
            throw new InvalidOperationException($"{matchingSpots.Count} spots match, should be 1");
        }

        var spot = matchingSpots.Single();
        if (spot.Registrable!.Type != RegistrableType.Double)
        {
            throw new InvalidOperationException("Switching is only possible in partner tracks.");
        }

        switch (command.ToRole)
        {
            case Role.Follower when spot.RegistrationId_Follower == null:
                // nobody in the other role -> just switch
                spot.RegistrationId = null;
                spot.RegistrationId_Follower = command.RegistrationId;
                break;

            case Role.Follower:
                // other role is taken -> new spot
                spots.InsertObjectTree(new Seat
                                       {
                                           Id = Guid.NewGuid(),
                                           RegistrableId = spot.RegistrableId,
                                           RegistrationId = null,
                                           RegistrationId_Follower = command.RegistrationId,
                                           FirstPartnerJoined = spot.Registration!.ReceivedAt,
                                           IsWaitingList = spot.IsWaitingList
                                       });
                spot.RegistrationId = null;
                break;

            case Role.Leader when spot.RegistrationId == null:
                // nobody in the other role -> just switch
                spot.RegistrationId = command.RegistrationId;
                spot.RegistrationId_Follower = null;
                break;

            case Role.Leader:
                // other role is taken -> new spot
                spots.InsertObjectTree(new Seat
                                       {
                                           Id = Guid.NewGuid(),
                                           RegistrableId = spot.RegistrableId,
                                           RegistrationId = command.RegistrationId,
                                           RegistrationId_Follower = null,
                                           FirstPartnerJoined = spot.Registration_Follower!.ReceivedAt,
                                           IsWaitingList = spot.IsWaitingList
                                       });
                spot.RegistrationId_Follower = null;
                break;
        }
    }
}