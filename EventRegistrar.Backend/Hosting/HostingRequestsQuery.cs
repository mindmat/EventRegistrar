using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Hosting
{
    public class HostingRequestsQuery : IRequest<HostingRequests>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }
}