using System;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Payments.Assignments
{
    public class PaymentAssigned : DomainEvent
    {
        public Guid PaymentId { get; set; }
        public Guid RegistrationId { get; set; }
    }
}