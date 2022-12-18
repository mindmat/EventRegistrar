using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Due;
using EventRegistrar.Backend.Registrations.ReadModels;

namespace EventRegistrar.Backend.Registrations.IndividualReductions;

public class RemoveIndividualReductionCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid ReductionId { get; set; }
}

public class RemoveIndividualReductionCommandHandler : IRequestHandler<RemoveIndividualReductionCommand>
{
    private readonly IEventBus _eventBus;
    private readonly ReadModelUpdater _readModelUpdater;
    private readonly IRepository<IndividualReduction> _reductions;

    public RemoveIndividualReductionCommandHandler(IRepository<IndividualReduction> reductions,
                                                   IEventBus eventBus,
                                                   ReadModelUpdater readModelUpdater)
    {
        _reductions = reductions;
        _eventBus = eventBus;
        _readModelUpdater = readModelUpdater;
    }

    public async Task<Unit> Handle(RemoveIndividualReductionCommand command, CancellationToken cancellationToken)
    {
        var reduction = await _reductions.FirstAsync(idr => idr.Id == command.ReductionId
                                                         && idr.Registration!.EventId == command.EventId, cancellationToken);

        _reductions.Remove(reduction);

        _eventBus.Publish(new IndividualReductionRemoved
                          {
                              RegistrationId = reduction.RegistrationId,
                              Amount = reduction.Amount
                          });

        _readModelUpdater.TriggerUpdate<RegistrationCalculator>(reduction.RegistrationId, command.EventId);
        _readModelUpdater.TriggerUpdate<DuePaymentsCalculator>(null, command.EventId);

        return Unit.Value;
    }
}