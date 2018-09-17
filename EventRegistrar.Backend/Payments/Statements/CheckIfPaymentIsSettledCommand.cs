using System;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using MediatR;

namespace EventRegistrar.Backend.Payments.Statements
{
    public class CheckIfPaymentIsSettledCommand : IRequest, IQueueBoundMessage
    {
        public Guid PaymentId { get; set; }
        public string QueueName => "CheckIfPaymentIsSettledCommandQueue";
    }
}