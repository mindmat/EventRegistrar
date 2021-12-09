using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;

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

    public DeleteRegistrableCommandHandler(IQueryable<Event> events,
                                           IRepository<Registrable> registrables)
    {
        _events = events;
        _registrables = registrables;
    }

    public async Task<Unit> Handle(DeleteRegistrableCommand command, CancellationToken cancellationToken)
    {
        var @event = await _events.FirstAsync(evt => evt.Id == command.EventId, cancellationToken);
        if (@event.State != RegistrationForms.State.Setup)
            throw new Exception(
                $"To delete a registrable, event must be in state Setup, but it is in state {@event.State}");

        var registrable = await _registrables.Where(rbl => rbl.Id == command.RegistrableId)
                                             .Include(rbl => rbl.Spots)
                                             .FirstAsync(cancellationToken);
        if (registrable.Spots.Any(spt => !spt.IsCancelled))
            throw new Exception("Registrable cannot be deleted because it contains registrations");

        _registrables.Remove(registrable);
        return Unit.Value;
    }
}