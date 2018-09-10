using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.Price;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Spots
{
    public class AddSpotCommandHandler : IRequestHandler<AddSpotCommand>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly PriceCalculator _priceCalculator;
        private readonly IQueryable<Registrable> _registrables;
        private readonly IQueryable<Registration> _registrations;
        private readonly IRepository<Seat> _seats;

        public AddSpotCommandHandler(IQueryable<Registration> registrations,
                                     IQueryable<Registrable> registrables,
                                     IRepository<Seat> seats,
                                     IEventAcronymResolver acronymResolver,
                                     PriceCalculator priceCalculator)
        {
            _registrations = registrations;
            _registrables = registrables;
            _seats = seats;
            _acronymResolver = acronymResolver;
            _priceCalculator = priceCalculator;
        }

        public async Task<Unit> Handle(AddSpotCommand command, CancellationToken cancellationToken)
        {
            var eventId = await _acronymResolver.GetEventIdFromAcronym(command.EventAcronym);
            var registration = await _registrations.FirstAsync(reg => reg.Id == command.RegistrationId
                                                                   && reg.EventId == eventId,
                                                               cancellationToken);
            var registrable = await _registrables.FirstAsync(rbl => rbl.Id == command.RegistrableId
                                                                 && rbl.EventId == eventId,
                                                             cancellationToken);
            var spot = new Seat
            {
                Id = Guid.NewGuid(),
                FirstPartnerJoined = DateTime.UtcNow,
                RegistrableId = registrable.Id,
                RegistrationId = registration.Id
            };
            await _seats.InsertOrUpdateEntity(spot, cancellationToken);
            //registration.Price = await _priceCalculator.CalculatePrice(registration.Id);
            //await PriceCalculator.CalculatePrice(registrationId, true, log);
            //await ServiceBusClient.SendEvent(new CheckIsWaitingListCommand { RegistrationId = registrationId }, CheckIsWaitingListCommandHandler.CheckIsWaitingListCommandsQueueName);
            return Unit.Value;
        }
    }
}