using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrables.Participants;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations.Matching;

public class UnbindPartnerSpotCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid SpotId { get; set; }
}

public class UnbindPartnerSpotCommandHandler(IRepository<Seat> spots,
                                      ChangeTrigger changeTrigger)
    : IRequestHandler<UnbindPartnerSpotCommand>
{
    public async Task Handle(UnbindPartnerSpotCommand command, CancellationToken cancellationToken)
    {
        var spotToUnbind = await spots.AsTracking()
                                      .FirstAsync(spt => spt.Registrable!.EventId == command.EventId
                                                      && spt.Id == command.SpotId,
                                                  cancellationToken);
        spotToUnbind.IsPartnerSpot = false;
        spotToUnbind.PartnerEmail = null;

        changeTrigger.QueryChanged<ParticipantsOfRegistrableQuery>(command.EventId, spotToUnbind.RegistrableId);
        changeTrigger.TriggerUpdate<RegistrablesOverviewCalculator>(null, command.EventId);
        changeTrigger.TriggerUpdate<RegistrationCalculator>(spotToUnbind.RegistrationId, command.EventId);
        changeTrigger.TriggerUpdate<RegistrationCalculator>(spotToUnbind.RegistrationId_Follower, command.EventId);
    }
}