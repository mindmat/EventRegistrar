using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Due;

namespace EventRegistrar.Backend.Registrations.IndividualReductions;

public class AddIndividualReductionCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid ReductionId { get; set; }
    public Guid RegistrationId { get; set; }
    public IndividualReductionType Type { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
}

public class AddIndividualReductionCommandHandler(IQueryable<Registration> registrations,
                                                  IRepository<IndividualReduction> reductions,
                                                  AuthenticatedUserId userId,
                                                  IEventBus eventBus,
                                                  ChangeTrigger changeTrigger)
    : IRequestHandler<AddIndividualReductionCommand>
{
    public async Task Handle(AddIndividualReductionCommand command, CancellationToken cancellationToken)
    {
        var registration = await registrations.FirstAsync(reg => reg.Id == command.RegistrationId
                                                              && reg.EventId == command.EventId, cancellationToken);

        var reduction = new IndividualReduction
                        {
                            Id = command.ReductionId,
                            RegistrationId = command.RegistrationId,
                            Type = command.Type,
                            Amount = command.Amount,
                            Reason = command.Reason,
                            UserId = userId.UserId!.Value
                        };
        reductions.InsertObjectTree(reduction);

        eventBus.Publish(new IndividualReductionAdded
                         {
                             RegistrationId = registration.Id,
                             Amount = command.Amount,
                             Reason = command.Reason
                         });

        changeTrigger.TriggerUpdate<DuePaymentsCalculator>(null, registration.EventId);
    }
}