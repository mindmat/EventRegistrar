using System;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using MediatR;

namespace EventRegistrar.Backend.Registrations.Confirmation
{
    public class CheckRegistrationAfterPaymentCommand : IRequest, IQueueBoundMessage
    {
        public string QueueName => "CheckRegistrationAfterPaymentCommandQueue";
        public Guid RegistrationId { get; set; }
    }
}