using System;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using MediatR;

namespace EventRegistrar.Backend.Payments.Files
{
    public class TryAssignPaymentSlipCommand : IRequest, IQueueBoundMessage
    {
        public Guid EventId { get; set; }
        public Guid PaymentSlipId { get; set; }
        public string QueueName => "TryAssignPaymentSlipQueue";
        public string Reference { get; set; }
    }
}