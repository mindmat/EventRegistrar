using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.Mailing.Import
{
    public class ImportedMail : Entity
    {
        public string ContentHtml { get; set; }
        public string ContentPlainText { get; set; }
        public DateTime Date { get; set; }
        public Guid EventId { get; set; }
        public DateTime Imported { get; set; }
        public string MessageIdentifier { get; set; }
        public string Recipients { get; set; }
        public ICollection<ImportedMailToRegistration> Registrations { get; set; }
        public string SenderMail { get; set; }
        public string SenderName { get; set; }
        public string SendGridMessageId { get; set; }
        public string Subject { get; set; }
    }
}