using System;
using EventRegistrar.Backend.Infrastructure.ServiceBus;

namespace EventRegistrar.Backend.Registrations.Register
{
    //[ProcessQueueMessage("processrawregistration")]
    public class ProcessRawRegistrationCommand : IQueueBoundMessage
    {
        public string QueueName => "processrawregistration";
        public Guid RawRegistrationId { get; set; }
    }
}