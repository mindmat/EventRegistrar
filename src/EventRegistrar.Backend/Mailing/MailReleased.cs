using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Mailing;

public class MailReleased : DomainEvent
{
    public Guid MailId { get; set; }
    public string To { get; set; }
    public string Subject { get; set; }
}

public class MailReleasedUserTranslation : IEventToUserTranslation<MailReleased>
{
    public string GetText(MailReleased domainEvent)
    {
        return $"Mail an {domainEvent.To} freigegeben, Betreff {domainEvent.Subject}";
    }
}