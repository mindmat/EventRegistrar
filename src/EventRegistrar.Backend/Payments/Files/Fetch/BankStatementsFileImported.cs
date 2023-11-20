using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Payments.Files.Fetch;

public class BankStatementsFileImported : DomainEvent
{
    public Guid BankStatementsFileId { get; set; }
}

public class PaymentUnassignedUserTranslation(IQueryable<RawBankStatementsFile> rawBankStatementsFiles) : IEventToUserTranslation<BankStatementsFileImported>
{
    public string GetText(BankStatementsFileImported domainEvent)
    {
        var file = rawBankStatementsFiles.FirstOrDefault(rbf => rbf.Id == domainEvent.BankStatementsFileId);
        return $"Datei {file?.Filename} importiert";
    }
}