using System;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrations.Confirmation
{
    public class PartnerRegistrationPaid : DomainEvent
    {
        public Guid RegistrationId1 { get; set; }
        public Guid RegistrationId2 { get; set; }
        public bool WillPayAtCheckin { get; set; }
    }
}