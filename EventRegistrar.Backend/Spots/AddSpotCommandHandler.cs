using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.Price;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Spots
{
    public class AddSpotCommandHandler : IRequestHandler<AddSpotCommand>
    {
        private readonly PriceCalculator _priceCalculator;
        private readonly IQueryable<Registration> _registrations;
        private readonly IRepository<Seat> _seats;

        public AddSpotCommandHandler(IQueryable<Registration> registrations,
            IRepository<Seat> seats,
            PriceCalculator priceCalculator)
        {
            _registrations = registrations;
            _seats = seats;
            _priceCalculator = priceCalculator;
        }

        public async Task<Unit> Handle(AddSpotCommand command, CancellationToken cancellationToken)
        {
            var registration = await _registrations.FirstOrDefaultAsync(reg => reg.Id == command.RegistrationId, cancellationToken);
            var spot = new Seat
            {
                Id = Guid.NewGuid(),
                FirstPartnerJoined = DateTime.UtcNow,
                RegistrableId = command.RegistrableId,
                RegistrationId = command.RegistrationId
            };
            await _seats.InsertOrUpdateEntity(spot, cancellationToken);
            //_priceCalculator.CalculatePrice(registration)
            //await PriceCalculator.CalculatePrice(registrationId, true, log);
            //await ServiceBusClient.SendEvent(new CheckIsWaitingListCommand { RegistrationId = registrationId }, CheckIsWaitingListCommandHandler.CheckIsWaitingListCommandsQueueName);
            return Unit.Value;
        }
    }
}