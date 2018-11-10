using System;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using MediatR;

namespace EventRegistrar.Backend.Registrables.WaitingList
{
    public class TryPromoteFromWaitingListCommand : IRequest, IEventBoundRequest, IQueueBoundMessage
    {
        public Guid EventId { get; set; }
        public string QueueName => "TryPromoteFromWaitingListCommandQueue";
        public Guid RegistrableId { get; set; }
    }
}