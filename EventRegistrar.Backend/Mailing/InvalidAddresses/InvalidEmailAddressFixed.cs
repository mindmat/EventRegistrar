using System;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Mailing.InvalidAddresses
{
    public class InvalidEmailAddressFixed : DomainEvent
    {
        public string NewEmailAddress { get; set; }
        public string OldEmailAddress { get; set; }
        public Guid RegistrationId { get; set; }
    }
}