using System;

using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Mailing.Import
{
    public class ImportedMailAssigned : DomainEvent
    {
        public DateTime ExternalDate { get; set; }
        public Guid ImportedMailId { get; set; }
        public Guid RegistrationId { get; set; }
        public string SenderMail { get; set; }
        public string SenderName { get; set; }
    }

    public class ImportedMailAssignedUserTranslation : IEventToUserTranslation<ImportedMailAssigned>
    {
        public string GetText(ImportedMailAssigned domainEvent)
        {
            return $"Mail erhalten von {domainEvent.SenderName} ({domainEvent.SenderMail})";
        }
    }
}