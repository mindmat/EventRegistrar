using System;
using System.Linq;

using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Payments.Files.Fetch
{
    public class BankStatementsFileImported : DomainEvent
    {
        public Guid BankStatementsFileId { get; set; }
    }

    public class PaymentUnassignedUserTranslation : IEventToUserTranslation<BankStatementsFileImported>
    {
        private readonly IQueryable<RawBankStatementsFile> _rawBankStatementsFiles;

        public PaymentUnassignedUserTranslation(IQueryable<RawBankStatementsFile> rawBankStatementsFiles)
        {
            _rawBankStatementsFiles = rawBankStatementsFiles;
        }

        public string GetText(BankStatementsFileImported domainEvent)
        {
            var file = _rawBankStatementsFiles.FirstOrDefault(rbf => rbf.Id == domainEvent.BankStatementsFileId);
            return $"Datei {file?.Filename} importiert";
        }
    }
}
