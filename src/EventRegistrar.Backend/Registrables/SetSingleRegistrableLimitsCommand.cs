using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;

namespace EventRegistrar.Backend.Registrables;

public class SetSingleRegistrableLimitsCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public int MaximumParticipants { get; set; }
    public Guid RegistrableId { get; set; }
}

public class SetSingleRegistrableLimitsCommandHandler : IRequestHandler<SetSingleRegistrableLimitsCommand>
{
    private readonly IRepository<Registrable> _registrables;

    public SetSingleRegistrableLimitsCommandHandler(IRepository<Registrable> registrables)
    {
        _registrables = registrables;
    }

    public async Task<Unit> Handle(SetSingleRegistrableLimitsCommand command, CancellationToken cancellationToken)
    {
        var registrable =
            await _registrables.FirstOrDefaultAsync(rbl => rbl.Id == command.RegistrableId, cancellationToken);
        if (registrable != null) registrable.MaximumSingleSeats = command.MaximumParticipants;

        return Unit.Value;
    }
}