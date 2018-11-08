using System;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrations.Confirmation
{
    public class SingleRegistrationPaid : Event
    {
        public Guid RegistrationId { get; set; }
    }
}