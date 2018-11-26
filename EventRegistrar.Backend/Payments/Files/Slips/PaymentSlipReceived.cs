using System;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Payments.Files.Slips
{
    public class PaymentSlipReceived : DomainEvent
    {
        public Guid PaymentSlipId { get; set; }
        public string Reference { get; set; }
    }
}