using System;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Mailing
{
    public class MailReleased : DomainEvent
    {
        public Guid MailId { get; internal set; }
        public string To { get; internal set; }
    }
}
