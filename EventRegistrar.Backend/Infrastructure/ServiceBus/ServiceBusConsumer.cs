using System;

namespace EventRegistrar.Backend.Infrastructure.ServiceBus
{
    public class ServiceBusConsumer
    {
        public string QueueName { get; set; }
        public Type RequestType { get; set; }
    }
}