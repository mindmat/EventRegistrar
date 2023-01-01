using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Spots;

public class RemoveSpotCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrableId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class RemoveSpotCommandHandler : AsyncRequestHandler<RemoveSpotCommand>
{
    private readonly IQueryable<Registration> _registrations;
    private readonly IRepository<Seat> _seats;
    private readonly SpotManager _spotManager;

    public RemoveSpotCommandHandler(IQueryable<Registration> registrations,
                                    IRepository<Seat> seats,
                                    SpotManager spotManager)
    {
        _registrations = registrations;
        _seats = seats;
        _spotManager = spotManager;
    }

    protected override async Task Handle(RemoveSpotCommand command, CancellationToken cancellationToken)
    {
        var registration = await _registrations.FirstAsync(reg => reg.Id == command.RegistrationId
                                                               && reg.EventId == command.EventId, cancellationToken);
        var spotToRemove = await _seats.FirstAsync(seat => seat.RegistrableId == command.RegistrableId
                                                        && !seat.IsCancelled
                                                        && (seat.RegistrationId == registration.Id
                                                         || seat.RegistrationId_Follower == registration.Id),
                                                   cancellationToken);

        _spotManager.RemoveSpot(spotToRemove, registration.Id, RemoveSpotReason.Modification);
    }
}