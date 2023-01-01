using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Payments.PayAtCheckin;

public class WillPayAtCheckinCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class WillPayAtCheckinCommandHandler : AsyncRequestHandler<WillPayAtCheckinCommand>
{
    private readonly IEventBus _eventBus;
    private readonly IRepository<Registration> _registrations;

    public WillPayAtCheckinCommandHandler(IRepository<Registration> registrations,
                                          IEventBus eventBus)
    {
        _registrations = registrations;
        _eventBus = eventBus;
    }

    protected override async Task Handle(WillPayAtCheckinCommand command, CancellationToken cancellationToken)
    {
        var registration = await _registrations.FirstAsync(reg => reg.Id == command.RegistrationId
                                                               && reg.EventId == command.EventId, cancellationToken);

        if (!registration.WillPayAtCheckin)
        {
            registration.WillPayAtCheckin = true;
            _eventBus.Publish(new WillPayAtCheckinSet
                              {
                                  EventId = registration.EventId,
                                  RegistrationId = registration.Id
                              });
        }
    }
}