using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Registrables.Participants;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrables;

public class DefragRegistrableCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrableId { get; set; }
}

public class DefragRegistrableCommandHandler(IQueryable<Registrable> registrables,
                                             ChangeTrigger changeTrigger)
    : IRequestHandler<DefragRegistrableCommand>
{
    public async Task Handle(DefragRegistrableCommand command, CancellationToken cancellationToken)
    {
        var registrableToCheck = await registrables.AsTracking()
                                                   .Where(rbl => rbl.Id == command.RegistrableId
                                                              && rbl.EventId == command.EventId)
                                                   .Include(rbl => rbl.Spots!.Where(spt => !spt.IsWaitingList && !spt.IsCancelled && !spt.IsPartnerSpot))
                                                   .FirstAsync(cancellationToken);
        var spots = registrableToCheck.Spots!.ToList();
        var registrationIds_Changed = new HashSet<Guid>();
        if (registrableToCheck.MaximumDoubleSeats != null)
        {
            var leaderSpots = spots.Where(spt => spt is { RegistrationId: not null, RegistrationId_Follower: null })
                                   .OrderBy(spt => spt.FirstPartnerJoined)
                                   .ToList();
            var followerSpots = new Queue<Seat>(spots.Where(spt => spt is { RegistrationId: null, RegistrationId_Follower: not null })
                                                     .OrderBy(spt => spt.FirstPartnerJoined));

            // fill the gaps
            foreach (var leaderSpot in leaderSpots)
            {
                if (!followerSpots.TryDequeue(out var followerSpot))
                {
                    break;
                }

                leaderSpot.RegistrationId_Follower = followerSpot.RegistrationId_Follower;
                registrationIds_Changed.Add(leaderSpot.RegistrationId!.Value);
                registrationIds_Changed.Add(leaderSpot.RegistrationId_Follower!.Value);
                followerSpot.IsCancelled = true;
            }
        }

        changeTrigger.TriggerUpdate<RegistrablesOverviewCalculator>(null, command.EventId);
        changeTrigger.QueryChanged<ParticipantsOfRegistrableQuery>(command.EventId, command.RegistrableId);
        foreach (var registrationId in registrationIds_Changed)
        {
            changeTrigger.TriggerUpdate<RegistrationCalculator>(registrationId, command.EventId);
        }
    }
}