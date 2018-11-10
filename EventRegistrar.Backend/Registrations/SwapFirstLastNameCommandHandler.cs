using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrations
{
    public class SwapFirstLastNameCommandHandler : IRequestHandler<SwapFirstLastNameCommand>
    {
        private readonly IRepository<Registration> _registrations;

        public SwapFirstLastNameCommandHandler(IRepository<Registration> registrations)
        {
            _registrations = registrations;
        }

        public async Task<Unit> Handle(SwapFirstLastNameCommand command, CancellationToken cancellationToken)
        {
            var registration = await _registrations.FirstAsync(reg => reg.Id == command.RegistrationId
                                                                      && reg.EventId == command.EventId, cancellationToken);
            var firstName = registration.RespondentFirstName;
            registration.RespondentFirstName = registration.RespondentLastName;
            registration.RespondentLastName = firstName;

            return Unit.Value;
        }
    }
}