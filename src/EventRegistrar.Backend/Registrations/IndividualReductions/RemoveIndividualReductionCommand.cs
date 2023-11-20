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

public class RemoveIndividualReductionCommandHandler(IRepository<IndividualReduction> reductions,
                                                     IEventBus eventBus,
                                                     ChangeTrigger changeTrigger)
    : IRequestHandler<RemoveIndividualReductionCommand>
{
    public async Task Handle(RemoveIndividualReductionCommand command, CancellationToken cancellationToken)
    {
        var reduction = await reductions.FirstAsync(idr => idr.Id == command.ReductionId
                                                        && idr.Registration!.EventId == command.EventId, cancellationToken);

        reductions.Remove(reduction);

        eventBus.Publish(new IndividualReductionRemoved
                         {
                             RegistrationId = reduction.RegistrationId,
                             Amount = reduction.Amount
                         });

        changeTrigger.TriggerUpdate<RegistrationCalculator>(reduction.RegistrationId, command.EventId);
        changeTrigger.TriggerUpdate<DuePaymentsCalculator>(null, command.EventId);
    }
}