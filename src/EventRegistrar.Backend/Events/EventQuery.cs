using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Events;

public class EventQuery : IRequest<EventDetails>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class EventQueryHandler : IRequestHandler<EventQuery, EventDetails>
{
    private readonly IQueryable<Event> _events;

    public EventQueryHandler(IQueryable<Event> events)
    {
        _events = events;
    }

    public async Task<EventDetails> Handle(EventQuery query, CancellationToken cancellationToken)
    {
        var @event = await _events.FirstAsync(evt => evt.Id == query.EventId, cancellationToken);
        return new EventDetails
               {
                   Id = @event.Id,
                   Name = @event.Name,
                   Acronym = @event.Acronym,
                   State = @event.State
               };
    }
}