using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrations.Price;

public class RecalculatePriceCommand : IRequest
{
    public Guid RegistrationId { get; set; }
}

public class RecalculatePriceCommandHandler : IRequestHandler<RecalculatePriceCommand>
{
    private readonly IEventBus _eventBus;
    private readonly PriceCalculator _priceCalculator;
    private readonly IRepository<Registration> _registrations;

    public RecalculatePriceCommandHandler(PriceCalculator priceCalculator,
                                          IRepository<Registration> registrations,
                                          IEventBus eventBus)
    {
        _priceCalculator = priceCalculator;
        _registrations = registrations;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(RecalculatePriceCommand command, CancellationToken cancellationToken)
    {
        var registration = await _registrations.FirstAsync(reg => reg.Id == command.RegistrationId, cancellationToken);
        var oldOriginal = registration.Price_Original;
        var oldAdmitted = registration.Price_Admitted;
        var oldAdmittedAndReduced = registration.Price_AdmittedAndReduced;

        var (newOriginal, newAdmitted, newAdmittedAndReduced, _, _) = await _priceCalculator.CalculatePrice(command.RegistrationId, cancellationToken);

        if (oldOriginal != newOriginal
         || oldAdmitted != newAdmitted
         || oldAdmittedAndReduced != newAdmittedAndReduced)
        {
            registration.Price_Original = newOriginal;
            registration.Price_Admitted = newAdmitted;
            registration.Price_AdmittedAndReduced = newAdmittedAndReduced;
            _eventBus.Publish(new PriceChanged
                              {
                                  EventId = registration.EventId,
                                  RegistrationId = registration.Id,
                                  OldPrice = oldAdmittedAndReduced,
                                  NewPrice = newAdmittedAndReduced
                              });
        }

        return Unit.Value;
    }
}