using System;
using EventRegistrar.Backend.Authorization;
using MediatR;

namespace EventRegistrar.Backend.Payments.Due
{
    public class SendReminderCommand : IRequest, IEventBoundRequest
    {
        public string EventAcronym { get; set; }
        public Guid RegistrationId { get; set; }
        public bool Withhold { get; set; }
    }
}