namespace EventRegistrar.Backend.Infrastructure.ServiceBus
{
    public interface IQueueBoundCommand
    {
        string QueueName { get; }
    }
}