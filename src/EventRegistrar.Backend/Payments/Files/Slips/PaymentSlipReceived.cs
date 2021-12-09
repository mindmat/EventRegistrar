using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Payments.Files.Slips;

public class PaymentSlipReceived : DomainEvent
{
    public Guid PaymentSlipId { get; set; }
    public string Reference { get; set; }
}

public class PaymentSlipReceivedUserTranslation : IEventToUserTranslation<PaymentSlipReceived>
{
    public string GetText(PaymentSlipReceived domainEvent)
    {
        return $"Referenz {domainEvent.Reference}";
    }
}