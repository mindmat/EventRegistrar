using System;
using EventRegistrar.Backend.Infrastructure.Events;

namespace EventRegistrar.Backend.Registrations.Cancel
{
    public class RegistrationCancelled : Event
    {
        public Guid RegistrationId { get; set; }
    }
}