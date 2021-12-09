using MediatR;

namespace EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;

public class RequestAccessCommand : IRequest<Guid>
{
    public Guid EventId { get; set; }
    public string RequestText { get; set; }
}