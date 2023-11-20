namespace EventRegistrar.Backend.Events;

public interface IEventAcronymResolver
{
    Task<Guid> GetEventIdFromAcronym(string eventAcronym);
}

internal class EventAcronymResolver(IQueryable<Event> events) : IEventAcronymResolver
{
    public async Task<Guid> GetEventIdFromAcronym(string eventAcronym)
    {
        var @event = await events.FirstOrDefaultAsync(evt => evt.Acronym == eventAcronym);
        if (@event == null)
        {
            throw new ArgumentOutOfRangeException($"There is no event {eventAcronym}");
        }

        return @event.Id;
    }
}