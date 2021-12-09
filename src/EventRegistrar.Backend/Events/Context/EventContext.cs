namespace EventRegistrar.Backend.Events.Context;

public class EventContext
{
    public Guid? EventId { get; set; }

    public static implicit operator Guid?(EventContext eventContext)
    {
        return eventContext.EventId;
    }
}