using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using MediatR;

namespace EventRegistrar.Backend.Registrables.WaitingList.Promotion;

public class ActivateAutomaticPromotionCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public bool TryPromoteImmediately { get; set; }
    public Guid RegistrableId { get; set; }
}

public class ActivateAutomaticPromotionCommandHandler : IRequestHandler<ActivateAutomaticPromotionCommand>
{
    private readonly IRepository<Registrable> _registrables;
    private readonly IEventBus _eventBus;

    public ActivateAutomaticPromotionCommandHandler(IRepository<Registrable> registrables,
                                                    IEventBus eventBus)
    {
        _registrables = registrables;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(ActivateAutomaticPromotionCommand command, CancellationToken cancellationToken)
    {
        var registrable = await _registrables.FirstAsync(rbl => rbl.Id == command.RegistrableId);
        if (registrable.AutomaticPromotionFromWaitingList)
            // already activated
            return Unit.Value;

        registrable.AutomaticPromotionFromWaitingList = true;
        if (command.TryPromoteImmediately)
            _eventBus.Publish(new AutomaticPromotionActivated { RegistrableId = command.RegistrableId });
        return Unit.Value;
    }
}