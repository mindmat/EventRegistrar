using System;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrations.Confirmation
{
    public class SingleRegistrationPaid : DomainEvent
    {
        public Guid RegistrationId { get; set; }
    }
}