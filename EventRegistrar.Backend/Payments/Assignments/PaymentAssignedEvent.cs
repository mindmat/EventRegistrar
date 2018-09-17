using System;
using EventRegistrar.Backend.Infrastructure.Events;

namespace EventRegistrar.Backend.Payments.Assignments
{
    public class PaymentAssignedEvent : Event
    {
        public Guid PaymentId { get; set; }
        public Guid RegistrationId { get; set; }
    }
}