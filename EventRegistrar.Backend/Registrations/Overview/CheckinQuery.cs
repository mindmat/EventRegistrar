using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Registrations.Overview
{
    public class CheckinQuery : IRequest<CheckinView>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }
}