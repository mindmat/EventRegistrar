using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.Price;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Spots
{
    public class RemoveSpotCommandHandler : IRequestHandler<RemoveSpotCommand>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly PriceCalculator _priceCalculator;
        private readonly IQueryable<Registration> _registrations;
        private readonly IRepository<Seat> _seats;
        private readonly SpotRemover _spotRemover;

        public RemoveSpotCommandHandler(IQueryable<Registration> registrations,
                                        IRepository<Seat> seats,
                                        PriceCalculator priceCalculator,
                                        SpotRemover spotRemover,
                                        IEventAcronymResolver acronymResolver)
        {
            _registrations = registrations;
            _seats = seats;
            _priceCalculator = priceCalculator;
            _spotRemover = spotRemover;
            _acronymResolver = acronymResolver;
        }

        public async Task<Unit> Handle(RemoveSpotCommand command, CancellationToken cancellationToken)
        {
            var eventId = await _acronymResolver.GetEventIdFromAcronym(command.EventAcronym);
            var registration = await _registrations.FirstAsync(reg => reg.Id == command.RegistrationId
                                                                   && reg.EventId == eventId, cancellationToken);
            var spotToRemove = await _seats.FirstOrDefaultAsync(seat => seat.RegistrableId == command.RegistrableId
                                                                     && seat.IsCancelled == false
                                                                     && (seat.RegistrationId == registration.Id
                                                                      || seat.RegistrationId_Follower == registration.Id),
                                                                cancellationToken);

            _spotRemover.RemoveSpot(spotToRemove, registration.Id);
            registration.Price = await _priceCalculator.CalculatePrice(registration.Id);

            //_priceCalculator.CalculatePrice(registration)
            //await PriceCalculator.CalculatePrice(registrationId, true, log);
            //await ServiceBusClient.SendEvent(new CheckIsWaitingListCommand { RegistrationId = registrationId }, CheckIsWaitingListCommandHandler.CheckIsWaitingListCommandsQueueName);
            return Unit.Value;
        }
    }
}