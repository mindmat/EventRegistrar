using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.ReadModels;

namespace EventRegistrar.Backend.Registrations.Remarks;

public class SetRemarksProcessedStateCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
    public bool NewProcessedState { get; set; }
}

public class SetRemarksProcessedStateCommandHandler(IRepository<Registration> registrations,
                                                    IEventBus eventBus,
                                                    ChangeTrigger changeTrigger)
    : IRequestHandler<SetRemarksProcessedStateCommand>
{
    private readonly IEventBus _eventBus = eventBus;

    public async Task Handle(SetRemarksProcessedStateCommand command, CancellationToken cancellationToken)
    {
        var registration = await registrations.AsTracking()
                                              .FirstAsync(reg => reg.Id == command.RegistrationId
                                                              && reg.EventId == command.EventId, cancellationToken);
        registration.RemarksProcessed = command.NewProcessedState;

        changeTrigger.QueryChanged<RemarksOverviewQuery>(command.EventId);
        changeTrigger.TriggerUpdate<RegistrationCalculator>(registration.Id, registration.EventId);
    }
}