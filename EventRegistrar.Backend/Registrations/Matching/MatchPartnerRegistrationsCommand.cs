using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Registrations.Matching
{
    public class MatchPartnerRegistrationsCommand : IEventBoundRequest, IRequest
    {
        public Guid EventId { get; set; }
        public Guid RegistrationId1 { get; set; }
        public Guid RegistrationId2 { get; set; }
    }
}