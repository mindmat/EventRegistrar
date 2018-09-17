using System;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using MediatR;

namespace EventRegistrar.Backend.Registrables.WaitingList
{
    public class CheckIfRegistrationIsPromotedCommand : IRequest, IQueueBoundMessage
    {
        public string QueueName => "CheckIfRegistrationIsPromotedCommandQueue";
        public Guid RegistrationId { get; set; }
    }
}