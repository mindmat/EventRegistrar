using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Mailing.Templates;

namespace EventRegistrar.Backend.Mailing
{
    public class Mail : Entity
    {
        public string BulkMailKey { get; set; }
        public string ContentHtml { get; set; }
        public string ContentPlainText { get; set; }
        public DateTime Created { get; set; }
        public bool Discarded { get; set; }
        public Guid? EventId { get; set; }
        public MailTemplate MailTemplate { get; set; }
        public Guid? MailTemplateId { get; set; }
        public string Recipients { get; set; }
        public ICollection<MailToRegistration> Registrations { get; set; }
        public string SenderMail { get; set; }
        public string SenderName { get; set; }
        public string SendGridMessageId { get; set; }
        public DateTime? Sent { get; set; }
        public MailState? State { get; set; }
        public string Subject { get; set; }
        public MailType? Type { get; set; }
        public bool Withhold { get; set; }
    }
}