using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Registrations.ReadModels;

namespace EventRegistrar.Backend.Registrations;

public class ChangeParticipantNameCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class ChangeParticipantNameCommandHandler : AsyncRequestHandler<ChangeParticipantNameCommand>
{
    private readonly IRepository<Registration> _registrations;
    private readonly ChangeTrigger _changeTrigger;

    public ChangeParticipantNameCommandHandler(IRepository<Registration> registrations, ChangeTrigger changeTrigger)
    {
        _registrations = registrations;
        _changeTrigger = changeTrigger;
    }

    protected override async Task Handle(ChangeParticipantNameCommand command, CancellationToken cancellationToken)
    {
        var registration = await _registrations.AsTracking()
                                               .FirstAsync(reg => reg.EventId == command.EventId
                                                               && reg.Id == command.RegistrationId, cancellationToken);
        registration.RespondentFirstName = command.FirstName;
        registration.RespondentLastName = command.LastName;

        _changeTrigger.TriggerUpdate<RegistrationCalculator>(registration.Id, registration.EventId);
    }
}