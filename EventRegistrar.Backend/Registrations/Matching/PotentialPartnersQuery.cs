using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Registrations.Matching
{
    public class PotentialPartnersQuery : IEventBoundRequest, IRequest<IEnumerable<PotentialPartnerMatch>>
    {
        public Guid EventId { get; set; }
        public Guid RegistrationId { get; set; }
        public string SearchString { get; set; }
    }
}