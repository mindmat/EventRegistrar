using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Payments.Files;

public class PaymentFileProcessed : DomainEvent
{
    public string Account { get; set; }
    public decimal Balance { get; set; }
    public int EntriesCount { get; set; }
}

public class PaymentFileProcessedUserTranslation : IEventToUserTranslation<PaymentFileProcessed>
{
    public string GetText(PaymentFileProcessed domainEvent)
    {
        return $"{domainEvent.EntriesCount} Kontobewegungen auf {domainEvent.Account}, Saldo {domainEvent.Balance}";
    }
}