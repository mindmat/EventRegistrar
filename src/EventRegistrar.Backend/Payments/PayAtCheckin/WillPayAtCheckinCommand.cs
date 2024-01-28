using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Payments.PayAtCheckin;

public class WillPayAtCheckinCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
    public bool WillPayAtCheckin { get; set; }
}

public class WillPayAtCheckinCommandHandler(
    IRepository<Registration> registrations,
    ChangeTrigger changeTrigger)
    : IRequestHandler<WillPayAtCheckinCommand>
{
    public async Task Handle(WillPayAtCheckinCommand command, CancellationToken cancellationToken)
    {
        var registration = await registrations.FirstAsync(reg => reg.Id == command.RegistrationId
                                                              && reg.EventId == command.EventId, cancellationToken);

        if (registration.WillPayAtCheckin != command.WillPayAtCheckin)
        {
            registration.WillPayAtCheckin = command.WillPayAtCheckin;
            changeTrigger.PublishEvent(new WillPayAtCheckinSet
                                       {
                                           EventId = registration.EventId,
                                           RegistrationId = registration.Id,
                                           WillPayAtCheckin = command.WillPayAtCheckin
                                       });
            changeTrigger.TriggerUpdate<RegistrationCalculator>(registration.Id, registration.EventId);
        }
    }
}