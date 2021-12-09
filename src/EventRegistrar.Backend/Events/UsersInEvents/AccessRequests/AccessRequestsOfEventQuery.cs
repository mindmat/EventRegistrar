using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;

public class AccessRequestsOfEventQuery : IRequest<IEnumerable<AccessRequestOfEvent>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public bool IncludeDeniedRequests { get; set; }
}