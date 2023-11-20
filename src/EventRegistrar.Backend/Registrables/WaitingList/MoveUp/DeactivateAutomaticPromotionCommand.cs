namespace EventRegistrar.Backend.Registrables.WaitingList.MoveUp;

public class DeactivateAutomaticPromotionCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrableId { get; set; }
}

public class DeactivateAutomaticPromotionCommandHandler(IRepository<Registrable> registrables) : IRequestHandler<DeactivateAutomaticPromotionCommand>
{
    public async Task Handle(DeactivateAutomaticPromotionCommand command, CancellationToken cancellationToken)
    {
        var registrable = await registrables.FirstAsync(rbl => rbl.Id == command.RegistrableId,
                                                        cancellationToken);
        if (registrable.AutomaticPromotionFromWaitingList == false)
        {
            // already activated
            return;
        }

        registrable.AutomaticPromotionFromWaitingList = false;
    }
}