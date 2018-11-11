using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Events
{
    public class OpenRegistrationCommand : IEventBoundRequest, IRequest
    {
        public bool DeleteData { get; set; }
        public Guid EventId { get; set; }
    }
}