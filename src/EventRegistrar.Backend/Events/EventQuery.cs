namespace EventRegistrar.Backend.Events;

public class EventQuery : IRequest<EventDetails>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class EventQueryHandler(IQueryable<Event> events) : IRequestHandler<EventQuery, EventDetails>
{
    public async Task<EventDetails> Handle(EventQuery query, CancellationToken cancellationToken)
    {
        var @event = await events.FirstAsync(evt => evt.Id == query.EventId, cancellationToken);
        return new EventDetails
               {
                   Id = @event.Id,
                   Name = @event.Name,
                   Acronym = @event.Acronym,
                   State = @event.State
               };
    }
}