using System;
using EventRegistrar.Backend.Infrastructure.Events;

namespace EventRegistrar.Backend.Registrations.Confirmation
{
    public class SingleRegistrationPaid : Event
    {
        public Guid RegistrationId { get; set; }
    }
}