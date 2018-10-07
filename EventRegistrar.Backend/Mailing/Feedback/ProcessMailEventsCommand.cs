using System;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using MediatR;

namespace EventRegistrar.Backend.Mailing.Feedback
{
    public class ProcessMailEventsCommand : IRequest, IQueueBoundMessage
    {
        public string QueueName => "processmailevents";
        public Guid RawMailEventsId { get; set; }
    }
}