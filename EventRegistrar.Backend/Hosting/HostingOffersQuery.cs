using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Hosting
{
    public class HostingOffersQuery : IRequest<HostingOffers>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }
}