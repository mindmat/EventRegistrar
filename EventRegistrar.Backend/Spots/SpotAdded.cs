using System;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Spots
{
    public class SpotAdded : DomainEvent
    {
        public Guid RegistrableId { get; set; }
        public Guid RegistrationId { get; set; }
    }
}