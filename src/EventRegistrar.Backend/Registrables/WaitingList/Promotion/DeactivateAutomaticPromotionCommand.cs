using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;

namespace EventRegistrar.Backend.Registrables.WaitingList.Promotion;

public class DeactivateAutomaticPromotionCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrableId { get; set; }
}

public class DeactivateAutomaticPromotionCommandHandler : IRequestHandler<DeactivateAutomaticPromotionCommand>
{
    private readonly IRepository<Registrable> _registrables;

    public DeactivateAutomaticPromotionCommandHandler(IRepository<Registrable> registrables)
    {
        _registrables = registrables;
    }

    public async Task<Unit> Handle(DeactivateAutomaticPromotionCommand command, CancellationToken cancellationToken)
    {
        var registrable = await _registrables.FirstAsync(rbl => rbl.Id == command.RegistrableId);
        if (registrable.AutomaticPromotionFromWaitingList == false)
            // already activated
            return Unit.Value;

        registrable.AutomaticPromotionFromWaitingList = false;
        return Unit.Value;
    }
}