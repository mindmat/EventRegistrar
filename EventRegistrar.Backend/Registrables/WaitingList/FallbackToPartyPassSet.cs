using System;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrables.WaitingList
{
    public class FallbackToPartyPassSet : DomainEvent
    {
        public Guid RegistrationId { get; set; }
    }
}