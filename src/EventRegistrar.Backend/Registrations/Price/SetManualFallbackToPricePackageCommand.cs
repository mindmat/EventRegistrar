using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrables.WaitingList;
using EventRegistrar.Backend.Registrations.ReadModels;

namespace EventRegistrar.Backend.Registrations.Price;

public class SetManualFallbackToPricePackageCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
    public IEnumerable<Guid>? PricePackageIds { get; set; }
}

public class SetManualFallbackToPricePackageCommandHandler(IRepository<Registration> registrations,
                                                           ChangeTrigger changeTrigger,
                                                           IEventBus eventBus)
    : IRequestHandler<SetManualFallbackToPricePackageCommand>
{
    public async Task Handle(SetManualFallbackToPricePackageCommand command, CancellationToken cancellationToken)
    {
        if (command.PricePackageIds == null)
        {
            return;
        }

        var registration = await registrations.AsTracking()
                                              .FirstAsync(reg => reg.Id == command.RegistrationId
                                                              && reg.EventId == command.EventId,
                                                          cancellationToken);
        var newPackageIds = command.PricePackageIds.OrderBy(ppk => ppk).ToList();
        if (!registration.PricePackageIds_ManualFallback.SequenceEqual(newPackageIds))
        {
            registration.PricePackageIds_ManualFallback = newPackageIds;
            changeTrigger.TriggerUpdate<RegistrationCalculator>(registration.Id, registration.EventId);
            eventBus.Publish(new ManualFallbackToPricePackageSet { RegistrationId = registration.Id, EventId = command.EventId });
        }
    }
}