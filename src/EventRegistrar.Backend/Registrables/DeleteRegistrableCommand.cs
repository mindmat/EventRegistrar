using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

namespace EventRegistrar.Backend.Registrables;

public class DeleteRegistrableCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrableId { get; set; }
}

public class DeleteRegistrableCommandHandler(IQueryable<Event> events,
                                             IRepository<Registrable> registrables,
                                             ChangeTrigger changeTrigger)
    : IRequestHandler<DeleteRegistrableCommand>
{
    public async Task Handle(DeleteRegistrableCommand command, CancellationToken cancellationToken)
    {
        var @event = await events.FirstAsync(evt => evt.Id == command.EventId, cancellationToken);
        if (@event.State != RegistrationForms.EventState.Setup)
        {
            throw new Exception($"To delete a registrable, event must be in state Setup, but it is in state {@event.State}");
        }

        var registrable = await registrables.Where(rbl => rbl.Id == command.RegistrableId
                                                       && rbl.EventId == command.EventId)
                                            .Include(rbl => rbl.Spots)
                                            .FirstAsync(cancellationToken);
        if (registrable.Spots!.Any(spt => !spt.IsCancelled))
        {
            throw new Exception("Registrable cannot be deleted because it contains registrations");
        }

        registrables.Remove(registrable);

        changeTrigger.TriggerUpdate<RegistrablesOverviewCalculator>(null, command.EventId);
    }
}