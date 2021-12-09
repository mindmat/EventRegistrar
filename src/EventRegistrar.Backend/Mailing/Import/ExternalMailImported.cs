using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Mailing.Import;

public class ExternalMailImported : DomainEvent
{
    public DateTime ExternalDate { get; set; }
    public Guid ImportedMailId { get; set; }
    public string Subject { get; set; }
    public string From { get; set; }
}

public class ExternalMailImportedUserTranslation : IEventToUserTranslation<ExternalMailImported>
{
    public string GetText(ExternalMailImported domainEvent)
    {
        return $"von {domainEvent.From}, Betreff {domainEvent.Subject}, erhalten um {domainEvent.ExternalDate}";
    }
}