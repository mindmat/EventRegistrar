namespace EventRegistrar.Backend.Mailing
{
    public interface IQueueBoundCommand
    {
        string QueueName { get; }
    }
}