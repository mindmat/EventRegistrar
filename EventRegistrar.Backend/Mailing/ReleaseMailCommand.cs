using System;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using MediatR;

namespace EventRegistrar.Backend.Mailing
{
    public class ReleaseMailCommand : IRequest, IEventBoundRequest, IQueueBoundMessage
    {
        public Guid EventId { get; set; }
        public Guid MailId { get; set; }
        public string QueueName => "ReleaseMail";
    }
}