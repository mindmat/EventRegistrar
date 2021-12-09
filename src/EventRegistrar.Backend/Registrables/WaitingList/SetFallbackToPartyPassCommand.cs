using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;
using MediatR;

namespace EventRegistrar.Backend.Registrables.WaitingList;

public class SetFallbackToPartyPassCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class SetFallbackToPartyPassCommandHandler : IRequestHandler<SetFallbackToPartyPassCommand>
{
    private readonly IEventBus _eventBus;
    private readonly IRepository<Registration> _registrations;

    public SetFallbackToPartyPassCommandHandler(IRepository<Registration> registrations,
                                                IEventBus eventBus)
    {
        _registrations = registrations;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(SetFallbackToPartyPassCommand command, CancellationToken cancellationToken)
    {
        var registration = await _registrations.FirstAsync(reg => reg.EventId == command.EventId
                                                               && reg.Id == command.RegistrationId, cancellationToken);

        if (registration.FallbackToPartyPass != true
         && registration.IsWaitingList == true
         && registration.State == RegistrationState.Received)
            registration.FallbackToPartyPass = true;

        _eventBus.Publish(new FallbackToPartyPassSet { RegistrationId = registration.Id, EventId = command.EventId });

        return Unit.Value;
    }
}