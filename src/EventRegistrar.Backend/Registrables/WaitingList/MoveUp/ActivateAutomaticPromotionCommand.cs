using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrables.WaitingList.MoveUp;

public class ActivateAutomaticPromotionCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public bool TryPromoteImmediately { get; set; }
    public Guid RegistrableId { get; set; }
}

public class ActivateAutomaticPromotionCommandHandler(IRepository<Registrable> registrables,
                                                      IEventBus eventBus)
    : IRequestHandler<ActivateAutomaticPromotionCommand>
{
    public async Task Handle(ActivateAutomaticPromotionCommand command, CancellationToken cancellationToken)
    {
        var registrable = await registrables.FirstAsync(rbl => rbl.Id == command.RegistrableId,
                                                        cancellationToken);
        if (registrable.AutomaticPromotionFromWaitingList)
        {
            // already activated
            return;
        }

        registrable.AutomaticPromotionFromWaitingList = true;
        if (command.TryPromoteImmediately)
        {
            eventBus.Publish(new AutomaticPromotionActivated { RegistrableId = command.RegistrableId });
        }
    }
}