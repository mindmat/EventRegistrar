using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Payments.PayAtCheckin;

public class WillPayAtCheckinCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class WillPayAtCheckinCommandHandler(IRepository<Registration> registrations,
                                            IEventBus eventBus)
    : IRequestHandler<WillPayAtCheckinCommand>
{
    public async Task Handle(WillPayAtCheckinCommand command, CancellationToken cancellationToken)
    {
        var registration = await registrations.FirstAsync(reg => reg.Id == command.RegistrationId
                                                              && reg.EventId == command.EventId, cancellationToken);

        if (!registration.WillPayAtCheckin)
        {
            registration.WillPayAtCheckin = true;
            eventBus.Publish(new WillPayAtCheckinSet
                             {
                                 EventId = registration.EventId,
                                 RegistrationId = registration.Id
                             });
        }
    }
}