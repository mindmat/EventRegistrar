using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.Registrables;

public class SetDoubleRegistrableLimitsCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public int MaximumCouples { get; set; }
    public int MaximumImbalance { get; set; }
    public Guid RegistrableId { get; set; }
}

public class SetDoubleRegistrableLimitsCommandHandler : IRequestHandler<SetDoubleRegistrableLimitsCommand>
{
    private readonly IRepository<Registrable> _registrables;

    public SetDoubleRegistrableLimitsCommandHandler(IRepository<Registrable> registrables)
    {
        _registrables = registrables;
    }

    public async Task<Unit> Handle(SetDoubleRegistrableLimitsCommand command, CancellationToken cancellationToken)
    {
        var registrable =
            await _registrables.FirstOrDefaultAsync(rbl => rbl.Id == command.RegistrableId, cancellationToken);
        if (registrable != null)
        {
            registrable.MaximumDoubleSeats = command.MaximumCouples;
            registrable.MaximumAllowedImbalance = command.MaximumImbalance;
        }

        return Unit.Value;
    }
}