using MediatR;

namespace EventRegistrar.Backend.Events.UsersInEvents.AccessRequests
{
    public class RequestAccessCommand : IRequest<Unit>
    {
        public string EventAcronym { get; set; }
        public string RequestText { get; set; }
    }
}