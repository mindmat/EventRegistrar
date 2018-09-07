using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrables
{
    public class SetDoubleRegistrableLimitsCommandHandler : IRequestHandler<SetDoubleRegistrableLimitsCommand>
    {
        private readonly IRepository<Registrable> _registrables;

        public SetDoubleRegistrableLimitsCommandHandler(IRepository<Registrable> registrables)
        {
            _registrables = registrables;
        }

        public async Task<Unit> Handle(SetDoubleRegistrableLimitsCommand command, CancellationToken cancellationToken)
        {
            var registrable = await _registrables.FirstOrDefaultAsync(rbl => rbl.Id == command.RegistrableId, cancellationToken);
            if (registrable != null)
            {
                registrable.MaximumDoubleSeats = command.MaximumCouples;
                registrable.MaximumAllowedImbalance = command.MaximumImbalance;
            }

            return Unit.Value;
        }
    }
}