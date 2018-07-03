using System;
using MediatR;

namespace EventRegistrar.Backend.Events.UsersInEvents.AccessRequests
{
    public class RespondToRequestCommand : IRequest<Unit>
    {
        public Guid AccessToEventRequestId { get; set; }
        public RequestResponse Response { get; set; }
        public string ResponseText { get; set; }
        public UserInEventRole Role { get; set; }
    }
}