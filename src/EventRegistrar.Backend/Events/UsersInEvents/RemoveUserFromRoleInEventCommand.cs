using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Events.UsersInEvents;

public class RemoveUserFromRoleInEventCommand : IRequest<Unit>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public UserInEventRole Role { get; set; }
    public Guid UserId { get; set; }
}