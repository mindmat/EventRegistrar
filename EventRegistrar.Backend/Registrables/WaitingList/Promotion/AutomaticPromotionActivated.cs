using EventRegistrar.Backend.Infrastructure.DomainEvents;
using System;

namespace EventRegistrar.Backend.Registrables.WaitingList.Promotion
{
    internal class AutomaticPromotionActivated : DomainEvent
    {
        public Guid RegistrableId { get; set; }
    }
}