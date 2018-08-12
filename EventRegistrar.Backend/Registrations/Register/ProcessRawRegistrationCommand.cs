using System;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using MediatR;

namespace EventRegistrar.Backend.Registrations.Register
{
    [ProcessQueueMessage("processrawregistration")]
    public class ProcessRawRegistrationCommand : IRequest
    {
        public Guid RawRegistrationId { get; set; }
    }
}