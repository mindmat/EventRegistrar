using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events.Context;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.Register;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Spots
{
    public class AddSpotCommandHandler : IRequestHandler<AddSpotCommand>
    {
        private readonly EventContext _eventContext;
        private readonly IQueryable<Registrable> _registrables;
        private readonly IQueryable<Registration> _registrations;
        private readonly SeatManager _seatManager;
        private readonly IRepository<Seat> _seats;

        public AddSpotCommandHandler(IQueryable<Registration> registrations,
                                     IQueryable<Registrable> registrables,
                                     IRepository<Seat> seats,
                                     SeatManager seatManager,
                                     EventContext eventContext)
        {
            _registrations = registrations;
            _registrables = registrables;
            _seats = seats;
            _seatManager = seatManager;
            _eventContext = eventContext;
        }

        public async Task<Unit> Handle(AddSpotCommand command, CancellationToken cancellationToken)
        {
            var registration = await _registrations.FirstAsync(reg => reg.Id == command.RegistrationId
                                                                   && reg.EventId == _eventContext.EventId,
                                                               cancellationToken);
            var registrable = await _registrables.Where(rbl => rbl.Id == command.RegistrableId
                                                            && rbl.EventId == _eventContext.EventId)
                                                 .Include(rbl => rbl.Seats)
                                                 .FirstAsync(cancellationToken);
            if (registrable.MaximumDoubleSeats.HasValue)
            {
                _seatManager.ReserveSinglePartOfPartnerSpot(_eventContext.EventId,
                                                            registrable,
                                                            registration.Id,
                                                            new RegistrationIdentification(registration),
                                                            null,
                                                            command.AsFollower ? Role.Follower : Role.Leader);
            }
            else
            {
                _seatManager.ReserveSingleSpot(_eventContext.EventId, registrable, registration.Id);
            }

            return Unit.Value;
        }
    }
}