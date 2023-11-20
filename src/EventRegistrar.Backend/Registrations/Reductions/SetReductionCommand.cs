using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrations.Reductions;

public class SetReductionCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public bool IsReduced { get; set; }
    public Guid RegistrationId { get; set; }
}

public class SetReductionCommandHandler(IRepository<Registration> registrations,
                                        IEventBus eventBus)
    : IRequestHandler<SetReductionCommand>
{
    public async Task Handle(SetReductionCommand command, CancellationToken cancellationToken)
    {
        var registration = await registrations.FirstAsync(reg => reg.Id == command.RegistrationId
                                                              && reg.EventId == command.EventId, cancellationToken);

        if (registration.IsReduced != command.IsReduced)
        {
            registration.IsReduced = command.IsReduced;
            eventBus.Publish(new ReductionChanged
                             {
                                 EventId = registration.EventId,
                                 RegistrationId = registration.Id,
                                 IsReduced = registration.IsReduced
                             });
        }
    }
}