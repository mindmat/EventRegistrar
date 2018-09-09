using System;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Mailing.Compose;

namespace EventRegistrar.Backend.Mailing.Templates
{
    public class MailTemplate : Entity
    {
        public string BulkMailKey { get; set; }
        public MailContentType ContentType { get; set; }
        public Guid EventId { get; set; }
        public string Language { get; set; }
        public MailingAudience? MailingAudience { get; set; }
        public string SenderMail { get; set; }
        public string SenderName { get; set; }
        public string Subject { get; set; }

        //public ContentType ContentType { get; set; }
        public string Template { get; set; }

        public MailType Type { get; set; }
    }
}