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
    private readonly ChangeTrigger _changeTrigger;
    private readonly IRepository<IndividualReduction> _reductions;

    public RemoveIndividualReductionCommandHandler(IRepository<IndividualReduction> reductions,
                                                   IEventBus eventBus,
                                                   ChangeTrigger changeTrigger)
    {
        _reductions = reductions;
        _eventBus = eventBus;
        _changeTrigger = changeTrigger;
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

        _changeTrigger.TriggerUpdate<RegistrationCalculator>(reduction.RegistrationId, command.EventId);
        _changeTrigger.TriggerUpdate<DuePaymentsCalculator>(null, command.EventId);

        return Unit.Value;
    }
}