using System;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Payments.Files
{
    public class PaymentSlipReceived : DomainEvent
    {
        public Guid PaymentSlipId { get; set; }
        public string Reference { get; set; }
    }
}