using System;

namespace EventRegistrar.Backend.Infrastructure.ServiceBus
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ProcessQueueMessageAttribute : Attribute
    {
        public ProcessQueueMessageAttribute(string queueName)
        {
            QueueName = queueName;
        }

        public string QueueName { get; }
    }
}