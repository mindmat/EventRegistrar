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
         || spotLeader.RegistrationId_Follower != null
         || spotLeader.IsCancelled)
        {
            throw new ArgumentException("Leader spot is not valid");
        }

        if (spotFollower.RegistrationId != null
         || spotFollower.RegistrationId_Follower == null
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

        if (spotLeader.RegistrationId_Follower == null)
        {
            spotLeader.RegistrationId_Follower = registrationId_Follower;
            spotLeader.IsPartnerSpot = true;
        }

        if (spotFollower.RegistrationId != null)
        {
            spotFollower.RegistrationId_Follower = null;
        }
        else
        {
            spotFollower.IsCancelled = true;
        }

        changeTrigger.QueryChanged<ParticipantsOfRegistrableQuery>(spotLeader.RegistrableId, command.EventId);
        changeTrigger.TriggerUpdate<RegistrablesOverviewCalculator>(null, command.EventId);
        changeTrigger.TriggerUpdate<RegistrationCalculator>(registrationId_Leader, command.EventId);
        changeTrigger.TriggerUpdate<RegistrationCalculator>(registrationId_Follower, command.EventId);
    }
}