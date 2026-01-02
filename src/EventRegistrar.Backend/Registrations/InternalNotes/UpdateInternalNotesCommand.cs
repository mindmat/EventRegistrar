using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Payments.Differences;
using EventRegistrar.Backend.Payments.Due;

namespace EventRegistrar.Backend.Registrations.InternalNotes;

public class UpdateInternalNotesCommand : IRequest<string?>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
    public string? Notes { get; set; }
}

public class UpdateInternalNotesCommandHandler(IRepository<Registration> registrations,
                                               ChangeTrigger changeTrigger)
    : IRequestHandler<UpdateInternalNotesCommand, string?>
{
    public async Task<string?> Handle(UpdateInternalNotesCommand command, CancellationToken cancellationToken)
    {
        var registration = await registrations.AsTracking()
                                              .FirstAsync(reg => reg.Id == command.RegistrationId
                                                              && reg.EventId == command.EventId, cancellationToken);
        registration.InternalNotes = string.IsNullOrWhiteSpace(command.Notes)
                                         ? null
                                         : command.Notes;

        changeTrigger.QueryChanged<InternalNotesQuery>(command.EventId);
        changeTrigger.QueryChanged<DifferencesQuery>(registration.EventId);
        changeTrigger.TriggerUpdate<RegistrationCalculator>(registration.Id, registration.EventId);
        changeTrigger.TriggerUpdate<DuePaymentsCalculator>(null, registration.EventId);

        return command.Notes;
    }
}