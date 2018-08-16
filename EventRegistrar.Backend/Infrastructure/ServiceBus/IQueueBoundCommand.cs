using MediatR;

namespace EventRegistrar.Backend.Infrastructure.ServiceBus
{
    public interface IQueueBoundCommand : IRequest
    {
        string QueueName { get; }
    }
}