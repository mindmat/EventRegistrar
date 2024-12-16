using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrables.Participants;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations.Matching;

public class MatchSingleSpotsCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid SpotId_Leader { get; set; }
    public Guid SpotId_Follower { get; set; }
}

public class MatchSingleSpotsCommandHandler(IRepository<Seat> spots,
                                            ChangeTrigger changeTrigger)
    : IRequestHandler<MatchSingleSpotsCommand>
{
    public async Task Handle(MatchSingleSpotsCommand command, CancellationToken cancellationToken)
    {
        var spotsToMatch = await spots.Where(spt => spt.Registrable!.EventId == command.EventId
                                                 && (spt.Id == command.SpotId_Leader
                                                  || spt.Id == command.SpotId_Follower))
                                      .ToListAsync(cancellationToken);
        var spotLeader = spotsToMatch.Single(spt => spt.Id == command.SpotId_Leader);
        var spotFollower = spotsToMatch.Single(spt => spt.Id == command.SpotId_Follower);
        if (spotLeader.RegistrationId == null
         || spotLeader.IsCancelled)
        {
            throw new ArgumentException("Leader spot is not valid");
        }

        if (spotFollower.RegistrationId_Follower == null
         || spotFollower.IsCancelled)
        {
            throw new ArgumentException("Leader spot is not valid");
        }

        var registrationId_Leader = spotLeader.RegistrationId.Value;
        var registrationId_Follower = spotFollower.RegistrationId_Follower.Value;

        if (spotLeader.RegistrableId != spotFollower.RegistrableId)
        {
            throw new ArgumentException("Spots are not in the same track");
        }

        if (spotLeader.Id == spotFollower.Id)
        {
            // coincidentally registrations to match could already be in the same partner spot
            spotLeader.IsPartnerSpot = true;
        }
        else if (spotLeader.RegistrationId_Follower == null)
        {
            // move follower to leader spot
            spotLeader.RegistrationId_Follower = spotFollower.RegistrationId_Follower;
            spotLeader.IsPartnerSpot = true;

            spotFollower.RegistrationId_Follower = null;
            if (spotFollower.RegistrationId == null)
            {
                // nobody left in spot
                spotFollower.IsCancelled = true;
            }
        }
        else if (spotFollower.RegistrationId == null)
        {
            // move leader to follower spot
            spotFollower.RegistrationId = spotLeader.RegistrationId;
            spotFollower.IsPartnerSpot = true;

            spotLeader.RegistrationId = null;
            if (spotLeader.RegistrationId_Follower == null)
            {
                // nobody left in spot
                spotLeader.IsCancelled = true;
            }
        }
        else
        {
            // both spots have other partners -> just swap around
            // move other follower to follower spot
            spotFollower.RegistrationId_Follower = spotLeader.RegistrationId_Follower;
            spotLeader.RegistrationId_Follower = registrationId_Follower;
            spotLeader.IsPartnerSpot = true;
        }
        
        changeTrigger.QueryChanged<ParticipantsOfRegistrableQuery>(command.EventId, spotLeader.RegistrableId);
        changeTrigger.TriggerUpdate<RegistrablesOverviewCalculator>(null, command.EventId);
        changeTrigger.TriggerUpdate<RegistrationCalculator>(registrationId_Leader, command.EventId);
        changeTrigger.TriggerUpdate<RegistrationCalculator>(registrationId_Follower, command.EventId);
    }
}