using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrables.WaitingList;
using EventRegistrar.Backend.Registrations.ReadModels;

namespace EventRegistrar.Backend.Registrations.Price;

public class SetManualFallbackToPricePackageCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
    public Guid? PricePackageId { get; set; }
}

public class SetManualFallbackToPricePackageCommandHandler(IRepository<Registration> registrations,
                                                           ChangeTrigger changeTrigger,
                                                           IEventBus eventBus)
    : IRequestHandler<SetManualFallbackToPricePackageCommand>
{
    public async Task Handle(SetManualFallbackToPricePackageCommand command, CancellationToken cancellationToken)
    {
        var registration = await registrations.AsTracking()
                                              .FirstAsync(reg => reg.Id == command.RegistrationId
                                                              && reg.EventId == command.EventId,
                                                          cancellationToken);

        if (registration.PricePackageId_ManualFallback != command.PricePackageId)
        {
            registration.PricePackageId_ManualFallback = command.PricePackageId;
            changeTrigger.TriggerUpdate<RegistrationCalculator>(registration.Id, registration.EventId);
            eventBus.Publish(new ManualFallbackToPricePackageSet { RegistrationId = registration.Id, EventId = command.EventId });
        }
    }
}