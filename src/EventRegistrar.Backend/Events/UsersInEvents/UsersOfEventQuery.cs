using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Events.UsersInEvents;

public class UsersOfEventQuery : IRequest<IEnumerable<UserInEventDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}