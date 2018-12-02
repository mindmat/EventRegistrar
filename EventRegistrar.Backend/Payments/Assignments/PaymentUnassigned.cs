using System;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Payments.Assignments
{
    public class PaymentUnassigned : DomainEvent
    {
        public Guid PaymentAssignmentId { get; set; }
        public Guid PaymentAssignmentId_Counter { get; set; }
        public Guid PaymentId { get; set; }
        public Guid RegistrationId { get; set; }
    }
}