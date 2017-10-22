using EventRegistrator.Functions.Infrastructure.DataAccess;
using System;

namespace EventRegistrator.Functions.Mailing
{
    public class MailTemplate : Entity
    {
        public Guid EventId { get; set; }
        public MailType Type { get; set; }
        public ContentType ContentType { get; set; }
        public string Template { get; set; }
        public string Language { get; set; }
        public string Subject { get; set; }
        public string SenderName { get; set; }
        public string SenderMail { get; set; }
    }
}