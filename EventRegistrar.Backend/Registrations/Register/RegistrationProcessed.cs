using System;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrations.Register
{
    public class RegistrationProcessed : DomainEvent
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string[] Registrables { get; set; }
        public Guid RegistrationId { get; set; }
    }
}