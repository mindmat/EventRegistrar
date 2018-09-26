using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.PhoneMessages
{
    public class SendSmsCommand : IRequest, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
        public string Message { get; set; }
        public Guid RegistrationId { get; set; }
    }
}