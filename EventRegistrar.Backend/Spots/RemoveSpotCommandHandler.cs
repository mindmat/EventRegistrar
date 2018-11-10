using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Spots
{
    public class RemoveSpotCommandHandler : IRequestHandler<RemoveSpotCommand>
    {
        private readonly IQueryable<Registration> _registrations;
        private readonly IRepository<Seat> _seats;
        private readonly SpotRemover _spotRemover;

        public RemoveSpotCommandHandler(IQueryable<Registration> registrations,
                                        IRepository<Seat> seats,
                                        SpotRemover spotRemover)
        {
            _registrations = registrations;
            _seats = seats;
            _spotRemover = spotRemover;
        }

        public async Task<Unit> Handle(RemoveSpotCommand command, CancellationToken cancellationToken)
        {
            var registration = await _registrations.FirstAsync(reg => reg.Id == command.RegistrationId
                                                                   && reg.EventId == command.EventId, cancellationToken);
            var spotToRemove = await _seats.FirstOrDefaultAsync(seat => seat.RegistrableId == command.RegistrableId
                                                                     && seat.IsCancelled == false
                                                                     && (seat.RegistrationId == registration.Id
                                                                      || seat.RegistrationId_Follower == registration.Id),
                                                                cancellationToken);

            _spotRemover.RemoveSpot(spotToRemove, registration.Id);

            return Unit.Value;
        }
    }
}