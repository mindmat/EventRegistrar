using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrations.InternalNotes;

public class UpdateInternalNotesCommand : IRequest<string?>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
    public string? Notes { get; set; }
}

public class UpdateInternalNotesCommandHandler(IRepository<Registration> registrations, IEventBus eventBus, ChangeTrigger changeTrigger) : IRequestHandler<UpdateInternalNotesCommand, string?>
{
    public async Task<string?> Handle(UpdateInternalNotesCommand command, CancellationToken cancellationToken)
    {
        var registration = await registrations.AsTracking()
                                              .FirstAsync(reg => reg.Id == command.RegistrationId
                                                              && reg.EventId == command.EventId, cancellationToken);
        registration.InternalNotes = string.IsNullOrWhiteSpace(command.Notes)
                                         ? null
                                         : command.Notes;

        eventBus.Publish(new QueryChanged
                         {
                             EventId = command.EventId,
                             QueryName = nameof(InternalNotesQuery)
                         });
        changeTrigger.TriggerUpdate<RegistrationCalculator>(registration.Id, registration.EventId);

        return command.Notes;
    }
}