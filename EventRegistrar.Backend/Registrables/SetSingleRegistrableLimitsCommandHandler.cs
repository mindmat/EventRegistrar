using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrables
{
    public class SetSingleRegistrableLimitsCommandHandler : IRequestHandler<SetSingleRegistrableLimitsCommand>
    {
        private readonly IRepository<Registrable> _registrables;

        public SetSingleRegistrableLimitsCommandHandler(IRepository<Registrable> registrables)
        {
            _registrables = registrables;
        }

        public async Task<Unit> Handle(SetSingleRegistrableLimitsCommand command, CancellationToken cancellationToken)
        {
            var registrable = await _registrables.FirstOrDefaultAsync(rbl => rbl.Id == command.RegistrableId, cancellationToken);
            if (registrable != null)
            {
                registrable.MaximumSingleSeats = command.MaximumParticipants;
            }

            return Unit.Value;
        }
    }
}