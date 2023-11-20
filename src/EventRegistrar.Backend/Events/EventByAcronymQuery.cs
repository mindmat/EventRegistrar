namespace EventRegistrar.Backend.Events;

public class EventByAcronymQuery : IRequest<EventDetails?>
{
    public string? EventAcronym { get; set; }
}

public class EventByAcronymQueryHandler(IQueryable<Event> events) : IRequestHandler<EventByAcronymQuery, EventDetails?>
{
    public async Task<EventDetails?> Handle(EventByAcronymQuery query, CancellationToken cancellationToken)
    {
        if (query.EventAcronym == null)
        {
            return null;
        }

        var @event = await events.FirstAsync(evt => evt.Acronym == query.EventAcronym.ToLowerInvariant(), cancellationToken);
        return new EventDetails
               {
                   Id = @event.Id,
                   Name = @event.Name,
                   Acronym = @event.Acronym,
                   State = @event.State
               };
    }
}