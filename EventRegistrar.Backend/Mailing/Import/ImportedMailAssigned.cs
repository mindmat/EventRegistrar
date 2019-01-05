using System;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Mailing.Import
{
    public class ImportedMailAssigned : DomainEvent
    {
        public DateTime ExternalDate { get; set; }
        public Guid ImportedMailId { get; set; }
        public Guid RegistrationId { get; set; }
    }
}