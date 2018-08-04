using System;
using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.Mailing.Templates
{
    public class MailTemplate : Entity
    {
        public Guid EventId { get; set; }
        public string Language { get; set; }
        public MailingAudience? MailingAudience { get; set; }
        public string MailingKey { get; set; }
        public string SenderMail { get; set; }
        public string SenderName { get; set; }
        public string Subject { get; set; }

        //public ContentType ContentType { get; set; }
        public string Template { get; set; }

        public MailType Type { get; set; }
    }
}