using System;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Registrables.WaitingList
{
    public class SingleSpotPromotedFromWaitingList : DomainEvent
    {
        public Guid RegistrableId { get; set; }
        public Guid RegistrationId { get; set; }
        public Role Role { get; set; }
    }
}