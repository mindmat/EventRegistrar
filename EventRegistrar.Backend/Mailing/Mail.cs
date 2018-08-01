using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.Mailing
{
    public class Mail : Entity
    {
        public MailType? Type { get; set; }
        public string ContentHtml { get; set; }
        public string ContentPlainText { get; set; }

        public string SenderMail { get; set; }
        public string SenderName { get; set; }
        public string Subject { get; set; }
        public string Recipients { get; set; }
        public DateTime Created { get; set; }
        public bool Withhold { get; set; }
        public ICollection<MailToRegistration> Registrations { get; set; }
        public string MailingKey { get; set; }
    }
}