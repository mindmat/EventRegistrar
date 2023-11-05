using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

namespace EventRegistrar.Backend.Registrables;

public class DeleteRegistrableCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrableId { get; set; }
}

public class DeleteRegistrableCommandHandler : IRequestHandler<DeleteRegistrableCommand>
{
    private readonly IQueryable<Event> _events;
    private readonly IRepository<Registrable> _registrables;
    private readonly ChangeTrigger _changeTrigger;

    public DeleteRegistrableCommandHandler(IQueryable<Event> events,
                                           IRepository<Registrable> registrables,
                                           ChangeTrigger changeTrigger)
    {
        _events = events;
        _registrables = registrables;
        _changeTrigger = changeTrigger;
    }

    public async Task<Unit> Handle(DeleteRegistrableCommand command, CancellationToken cancellationToken)
    {
        var @event = await _events.FirstAsync(evt => evt.Id == command.EventId, cancellationToken);
        if (@event.State != RegistrationForms.EventState.Setup)
        {
            throw new Exception($"To delete a registrable, event must be in state Setup, but it is in state {@event.State}");
        }

        var registrable = await _registrables.Where(rbl => rbl.Id == command.RegistrableId
                                                        && rbl.EventId == command.EventId)
                                             .Include(rbl => rbl.Spots)
                                             .FirstAsync(cancellationToken);
        if (registrable.Spots!.Any(spt => !spt.IsCancelled))
        {
            throw new Exception("Registrable cannot be deleted because it contains registrations");
        }

        _registrables.Remove(registrable);

        _changeTrigger.TriggerUpdate<RegistrablesOverviewCalculator>(null, command.EventId);

        return Unit.Value;
    }
}