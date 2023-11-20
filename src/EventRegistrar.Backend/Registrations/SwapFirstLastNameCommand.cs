namespace EventRegistrar.Backend.Registrations;

public class SwapFirstLastNameCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class SwapFirstLastNameCommandHandler(IRepository<Registration> registrations) : IRequestHandler<SwapFirstLastNameCommand>
{
    public async Task Handle(SwapFirstLastNameCommand command, CancellationToken cancellationToken)
    {
        var registration = await registrations.AsTracking()
                                              .FirstAsync(reg => reg.Id == command.RegistrationId
                                                              && reg.EventId == command.EventId, cancellationToken);
        (registration.RespondentFirstName, registration.RespondentLastName) = (registration.RespondentLastName, registration.RespondentFirstName);
    }
}