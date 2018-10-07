using System;
using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.Mailing.Feedback
{
    public class MailEvent : Entity
    {
        public DateTime Created { get; set; }
        public string ExternalIdentifier { get; set; }
        public Guid MailId { get; set; }
        public string RawEvent { get; set; }
        public MailState State { get; set; }
    }
}