namespace EventRegistrar.Backend.Events;

public class EventByAcronymQuery : IRequest<EventDetails?>
{
    public string? EventAcronym { get; set; }
}

public class EventByAcronymQueryHandler : IRequestHandler<EventByAcronymQuery, EventDetails?>
{
    private readonly IQueryable<Event> _events;

    public EventByAcronymQueryHandler(IQueryable<Event> events)
    {
        _events = events;
    }

    public async Task<EventDetails?> Handle(EventByAcronymQuery query, CancellationToken cancellationToken)
    {
        if (query.EventAcronym == null)
        {
            return null;
        }

        var @event = await _events.FirstAsync(evt => evt.Acronym == query.EventAcronym.ToLowerInvariant(), cancellationToken);
        return new EventDetails
               {
                   Id = @event.Id,
                   Name = @event.Name,
                   Acronym = @event.Acronym,
                   State = @event.State
               };
    }
}