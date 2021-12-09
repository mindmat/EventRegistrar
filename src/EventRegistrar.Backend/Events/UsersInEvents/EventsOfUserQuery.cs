using MediatR;

namespace EventRegistrar.Backend.Events.UsersInEvents;

public class EventsOfUserQuery : IRequest<IEnumerable<UserInEventDisplayItem>>
{
    public bool IncludeRequestedEvents { get; set; }
}