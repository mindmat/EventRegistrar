using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrations.Remarks;

public class SetRemarksProcessedStateCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
    public bool NewProcessedState { get; set; }
}

public class SetRemarksProcessedStateCommandHandler : AsyncRequestHandler<SetRemarksProcessedStateCommand>
{
    private readonly IRepository<Registration> _registrations;
    private readonly IEventBus _eventBus;

    public SetRemarksProcessedStateCommandHandler(IRepository<Registration> registrations,
                                                  IEventBus eventBus)
    {
        _registrations = registrations;
        _eventBus = eventBus;
    }

    protected override async Task Handle(SetRemarksProcessedStateCommand command, CancellationToken cancellationToken)
    {
        var registration = await _registrations.AsTracking()
                                               .FirstAsync(reg => reg.Id == command.RegistrationId
                                                               && reg.EventId == command.EventId, cancellationToken);
        registration.RemarksProcessed = command.NewProcessedState;

        _eventBus.Publish(new QueryChanged
                          {
                              EventId = command.EventId,
                              QueryName = nameof(RemarksOverviewQuery)
                          });
    }
}