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
        public string Registrable { get; set; }
        public string Participant { get; set; }
    }

    public class ExternalMailImportedUserTranslation : IEventToUserTranslation<SingleSpotPromotedFromWaitingList>
    {
        public string GetText(SingleSpotPromotedFromWaitingList domainEvent)
        {
            return $"{domainEvent.Participant} ist von der Warteliste von {domainEvent.Registrable} nachgerückt.";
        }
    }
}