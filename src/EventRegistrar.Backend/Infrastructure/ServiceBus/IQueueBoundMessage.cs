using MediatR;

namespace EventRegistrar.Backend.Infrastructure.ServiceBus;

public interface IQueueBoundMessage : IRequest
{
    string QueueName { get; }
}