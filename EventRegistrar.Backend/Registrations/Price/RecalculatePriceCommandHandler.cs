using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrations.Price
{
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
            var registration = await _registrations.Where(reg => reg.Id == command.RegistrationId)
                                                   .Include(reg => reg.IndividualReductions)
                                                   .FirstAsync(cancellationToken);
            var oldPrice = registration.Price ?? 0m;
            var individualReductions = registration.IndividualReductions
                                                   .Select(ird => ird.Amount)
                                                   .Sum();
            var oldOriginalPrice = registration.OriginalPrice ?? 0m;
            var newOriginalPrice = await _priceCalculator.CalculatePrice(command.RegistrationId);
            var newPrice = newOriginalPrice - individualReductions;
            if (oldOriginalPrice != newOriginalPrice || oldPrice != newPrice)
            {
                registration.OriginalPrice = newOriginalPrice;
                registration.Price = newPrice;
                _eventBus.Publish(new PriceChanged
                {
                    EventId = registration.EventId,
                    RegistrationId = registration.Id,
                    OldPrice = oldPrice,
                    NewPrice = registration.Price ?? 0m
                });
            }

            return Unit.Value;
        }
    }
}