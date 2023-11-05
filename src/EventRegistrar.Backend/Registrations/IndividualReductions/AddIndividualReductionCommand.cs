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

public class AddIndividualReductionCommandHandler : IRequestHandler<AddIndividualReductionCommand>
{
    private readonly IEventBus _eventBus;
    private readonly ChangeTrigger _changeTrigger;
    private readonly IRepository<IndividualReduction> _reductions;
    private readonly IQueryable<Registration> _registrations;
    private readonly AuthenticatedUserId _userId;

    public AddIndividualReductionCommandHandler(IQueryable<Registration> registrations,
                                                IRepository<IndividualReduction> reductions,
                                                AuthenticatedUserId userId,
                                                IEventBus eventBus,
                                                ChangeTrigger changeTrigger)
    {
        _registrations = registrations;
        _reductions = reductions;
        _userId = userId;
        _eventBus = eventBus;
        _changeTrigger = changeTrigger;
    }

    public async Task<Unit> Handle(AddIndividualReductionCommand command, CancellationToken cancellationToken)
    {
        var registration = await _registrations.FirstAsync(reg => reg.Id == command.RegistrationId
                                                               && reg.EventId == command.EventId, cancellationToken);

        var reduction = new IndividualReduction
                        {
                            Id = command.ReductionId,
                            RegistrationId = command.RegistrationId,
                            Type = command.Type,
                            Amount = command.Amount,
                            Reason = command.Reason,
                            UserId = _userId.UserId!.Value
                        };
        _reductions.InsertObjectTree(reduction);

        _eventBus.Publish(new IndividualReductionAdded
                          {
                              RegistrationId = registration.Id,
                              Amount = command.Amount,
                              Reason = command.Reason
                          });

        _changeTrigger.TriggerUpdate<DuePaymentsCalculator>(null, registration.EventId);

        return Unit.Value;
    }
}