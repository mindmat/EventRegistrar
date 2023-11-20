namespace EventRegistrar.Backend.Registrables.Reductions;

public class DeleteReductionCommand : IRequest, IEventBoundRequest
{
    public Guid RegistrableId { get; set; }
    public Guid ReductionId { get; set; }
    public Guid EventId { get; set; }
}

public class DeleteReductionCommandHandler(IRepository<Reduction> reductions) : IRequestHandler<DeleteReductionCommand>
{
    public async Task Handle(DeleteReductionCommand command, CancellationToken cancellationToken)
    {
        var reduction = await reductions.FirstAsync(red => red.Id == command.ReductionId,
                                                    cancellationToken);
        if (reduction.RegistrableId != command.RegistrableId)
        {
            throw new ArgumentException(
                $"Invalid data, reduction {command.ReductionId} does not belong to registrable {command.RegistrableId}");
        }

        reductions.Remove(reduction);
    }
}