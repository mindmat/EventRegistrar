using System;

using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Mailing
{
    public class MailReleased : DomainEvent
    {
        public Guid MailId { get; internal set; }
        public string To { get; internal set; }
        public string Subject { get; set; }
    }

    public class MailReleasedUserTranslation : IEventToUserTranslation<MailReleased>
    {
        public string GetText(MailReleased domainEvent)
        {
            return $"Mail an {domainEvent.To} freigegeben, Betreff {domainEvent.Subject}";
        }
    }
}
