using System;
using MediatR;

namespace EventRegistrar.Backend.Events.UsersInEvents.AccessRequests
{
    public class RequestAccessCommand : IRequest<Guid>
    {
        public string EventAcronym { get; set; }
        public string RequestText { get; set; }
    }
}