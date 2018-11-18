using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Registrations.Matching
{
    public class ChangeUnmatchedPartnerRegistrationToSingleRegistrationCommand : IRequest, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public Guid RegistrationId { get; set; }
    }
}