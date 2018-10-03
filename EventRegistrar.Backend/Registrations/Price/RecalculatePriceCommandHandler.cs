using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrations.Price
{
    public class RecalculatePriceCommandHandler : IRequestHandler<RecalculatePriceCommand>
    {
        private readonly EventBus _eventBus;
        private readonly PriceCalculator _priceCalculator;
        private readonly IRepository<Registration> _registrations;

        public RecalculatePriceCommandHandler(PriceCalculator priceCalculator,
                                              IRepository<Registration> registrations,
                                              EventBus eventBus)
        {
            _priceCalculator = priceCalculator;
            _registrations = registrations;
            _eventBus = eventBus;
        }

        public async Task<Unit> Handle(RecalculatePriceCommand command, CancellationToken cancellationToken)
        {
            var registration = await _registrations.FirstAsync(reg => reg.Id == command.RegistrationId, cancellationToken);
            var oldPrice = registration.Price ?? 0m;
            var newPrice = await _priceCalculator.CalculatePrice(command.RegistrationId);
            if (oldPrice != newPrice)
            {
                registration.Price = newPrice;
                _eventBus.Publish(new PriceChanged { RegistrationId = registration.Id, OldPrice = oldPrice, NewPrice = newPrice });
            }

            return Unit.Value;
        }
    }
}