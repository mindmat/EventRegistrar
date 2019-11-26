using System;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Payments.Files.Fetch
{
    public class BankStatementsFileImported : DomainEvent
    {
        public Guid BankStatementsFileId { get; set; }
    }
}
