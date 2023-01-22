using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Registrations.ReadModels;

namespace EventRegistrar.Backend.Registrations.Price;

public class SetManualFallbackToPricePackageCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
    public Guid? PricePackageId { get; set; }
}

public class SetManualFallbackToPricePackageCommandHandler : AsyncRequestHandler<SetManualFallbackToPricePackageCommand>
{
    private readonly IRepository<Registration> _registrations;
    private readonly ReadModelUpdater _readModelUpdater;

    public SetManualFallbackToPricePackageCommandHandler(IRepository<Registration> registrations,
                                                         ReadModelUpdater readModelUpdater)
    {
        _registrations = registrations;
        _readModelUpdater = readModelUpdater;
    }

    protected override async Task Handle(SetManualFallbackToPricePackageCommand command, CancellationToken cancellationToken)
    {
        var registration = await _registrations.AsTracking()
                                               .FirstAsync(reg => reg.Id == command.RegistrationId
                                                               && reg.EventId == command.EventId,
                                                           cancellationToken);

        if (registration.PricePackageId_ManualFallback != command.PricePackageId)
        {
            registration.PricePackageId_ManualFallback = command.PricePackageId;
            _readModelUpdater.TriggerUpdate<RegistrationCalculator>(registration.Id, registration.EventId);
        }
    }
}