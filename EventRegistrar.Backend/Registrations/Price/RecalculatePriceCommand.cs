using System;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using MediatR;

namespace EventRegistrar.Backend.Registrations.Price
{
    public class RecalculatePriceCommand : IRequest, IQueueBoundMessage
    {
        public string QueueName => "RecalculatePriceCommandQueue";
        public Guid RegistrationId { get; set; }
    }
}