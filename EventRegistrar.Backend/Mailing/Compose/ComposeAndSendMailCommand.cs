using System;
using EventRegistrar.Backend.Infrastructure.ServiceBus;

namespace EventRegistrar.Backend.Mailing.Compose
{
    public class ComposeAndSendMailCommand : IQueueBoundCommand
    {
        public bool AllowDuplicate { get; set; }
        public MailType MailType { get; set; }
        public string QueueName => "ComposeAndSendMailCommandQueue";
        public Guid RegistrationId { get; set; }
        public bool Withhold { get; set; }
    }
}