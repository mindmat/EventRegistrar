using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Infrastructure.ServiceBus;

namespace EventRegistrar.Backend.Mailing.Send
{
    public class SendMailCommand : IQueueBoundCommand
    {
        public string ContentHtml { get; set; }
        public string ContentPlainText { get; set; }
        public Guid MailId { get; set; }
        public string QueueName => "sendmailcommandqueue";
        public EmailAddress Sender { get; set; }
        public string Subject { get; set; }
        public IEnumerable<EmailAddress> To { get; set; }
    }
}