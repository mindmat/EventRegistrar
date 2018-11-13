using System;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrables.WaitingList
{
    public class PartnerSpotPromotedFromWaitingList : DomainEvent
    {
        public Guid RegistrableId { get; set; }
        public Guid? RegistrationId { get; set; }
        public Guid? RegistrationId_Follower { get; set; }
    }
}