using System;
using EventRegistrar.Backend.Infrastructure.Events;

namespace EventRegistrar.Backend.Registrations.Confirmation
{
    public class PartnerRegistrationPaid : Event
    {
        public Guid RegistrationId1 { get; set; }
        public Guid RegistrationId2 { get; set; }
    }
}