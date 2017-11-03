using System;
using System.Collections.Generic;

namespace EventRegistrator.Functions.Mailing
{
    public class SendMailCommand
    {
        public Guid MailId { get; set; }
        public string ContentHtml { get; set; }
        public string ContentPlainText { get; set; }

        public EmailAddress Sender { get; set; }
        public IEnumerable<EmailAddress> To { get; set; }
        public string Subject { get; set; }
    }
}