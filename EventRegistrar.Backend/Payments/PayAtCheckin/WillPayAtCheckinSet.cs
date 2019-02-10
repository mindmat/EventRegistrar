using System;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Payments.PayAtCheckin
{
    public class WillPayAtCheckinSet : DomainEvent
    {
        public Guid RegistrationId { get; set; }
    }
}