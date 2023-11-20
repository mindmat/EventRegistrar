using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Spots;

public class RemoveSpotCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrableId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class RemoveSpotCommandHandler(IQueryable<Registration> registrations,
                                      IRepository<Seat> seats,
                                      SpotManager spotManager)
    : IRequestHandler<RemoveSpotCommand>
{
    public async Task Handle(RemoveSpotCommand command, CancellationToken cancellationToken)
    {
        var registration = await registrations.FirstAsync(reg => reg.Id == command.RegistrationId
                                                              && reg.EventId == command.EventId, cancellationToken);
        var spotToRemove = await seats.FirstAsync(seat => seat.RegistrableId == command.RegistrableId
                                                       && !seat.IsCancelled
                                                       && (seat.RegistrationId == registration.Id
                                                        || seat.RegistrationId_Follower == registration.Id),
                                                  cancellationToken);

        spotManager.RemoveSpot(spotToRemove, registration.Id, RemoveSpotReason.Modification);
    }
}