using System;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Mailing.Import
{
    public class ExternalMailImported : DomainEvent
    {
        public DateTime ExternalDate { get; set; }
        public Guid ImportedMailId { get; set; }
    }
}