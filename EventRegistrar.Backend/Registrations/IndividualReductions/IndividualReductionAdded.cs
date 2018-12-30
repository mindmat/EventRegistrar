using System;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrations.IndividualReductions
{
    public class IndividualReductionAdded : DomainEvent
    {
        public decimal Amount { get; set; }
        public Guid RegistrationId { get; set; }
    }
}