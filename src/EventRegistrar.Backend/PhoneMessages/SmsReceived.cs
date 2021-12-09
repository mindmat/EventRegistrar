using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.PhoneMessages;

public class SmsReceived : DomainEvent
{
    public Guid? RegistrationId { get; set; }
    public string Registration { get; set; }
    public string From { get; set; }
    public string Text { get; set; }
    public DateTimeOffset Received { get; set; }
}

public class SmsReceivedUserTranslation : IEventToUserTranslation<SmsReceived>
{
    public string GetText(SmsReceived domainEvent)
    {
        return $"SMS erhalten von {domainEvent.Registration} ({domainEvent.From}): \"{domainEvent.Text}\"";
    }
}