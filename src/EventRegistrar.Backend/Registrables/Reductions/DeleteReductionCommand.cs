using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;

namespace EventRegistrar.Backend.Registrables.Reductions;

public class DeleteReductionCommand : IRequest, IEventBoundRequest
{
    public Guid RegistrableId { get; set; }
    public Guid ReductionId { get; set; }
    public Guid EventId { get; set; }
}

public class DeleteReductionCommandHandler : IRequestHandler<DeleteReductionCommand>
{
    private readonly IRepository<Reduction> _reductions;

    public DeleteReductionCommandHandler(IRepository<Reduction> reductions)
    {
        _reductions = reductions;
    }

    public async Task<Unit> Handle(DeleteReductionCommand command, CancellationToken cancellationToken)
    {
        var reduction = await _reductions.FirstAsync(red => red.Id == command.ReductionId);
        if (reduction.RegistrableId != command.RegistrableId)
            throw new ArgumentException(
                $"Invalid data, reduction {command.ReductionId} does not belong to registrable {command.RegistrableId}");

        _reductions.Remove(reduction);
        return Unit.Value;
    }
}