using System;

using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrations.Cancel
{
    public class RegistrationCancelled : DomainEvent
    {
        public Guid RegistrationId { get; set; }
        public string Reason { get; set; }
        public decimal Refund { get; set; }
        public DateTimeOffset Received { get; set; }
        public string Participant { get; set; }
    }

    public class ExternalMailImportedUserTranslation : IEventToUserTranslation<RegistrationCancelled>
    {
        public string GetText(RegistrationCancelled domainEvent)
        {
            return $"Teilnehmer {domainEvent.Participant}, Grund {domainEvent.Reason}, eingegangen {domainEvent.Received}, Rückerstattung {domainEvent.Refund}";
        }
    }
}