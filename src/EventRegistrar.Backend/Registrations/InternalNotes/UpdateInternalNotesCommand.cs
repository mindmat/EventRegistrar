using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.ReadModels;

namespace EventRegistrar.Backend.Registrations.InternalNotes;

public class UpdateInternalNotesCommand : IRequest<string?>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
    public string? Notes { get; set; }
}

public class UpdateInternalNotesCommandHandler : IRequestHandler<UpdateInternalNotesCommand, string?>
{
    private readonly IRepository<Registration> _registrations;
    private readonly IEventBus _eventBus;
    private readonly ReadModelUpdater _readModelUpdater;

    public UpdateInternalNotesCommandHandler(IRepository<Registration> registrations, IEventBus eventBus, ReadModelUpdater readModelUpdater)
    {
        _registrations = registrations;
        _eventBus = eventBus;
        _readModelUpdater = readModelUpdater;
    }

    public async Task<string?> Handle(UpdateInternalNotesCommand command, CancellationToken cancellationToken)
    {
        var registration = await _registrations.AsTracking()
                                               .FirstAsync(reg => reg.Id == command.RegistrationId
                                                               && reg.EventId == command.EventId, cancellationToken);
        registration.InternalNotes = command.Notes;

        //_eventBus.Publish(new QueryChanged
        //                  {
        //                      EventId = command.EventId,
        //                      QueryName = nameof(RemarksOverviewQuery)
        //                  });
        _readModelUpdater.TriggerUpdate<RegistrationCalculator>(registration.Id, registration.EventId);

        return command.Notes;
    }
}