using System;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrations.Reductions
{
    public class ReductionChanged : DomainEvent
    {
        public bool IsReduced { get; set; }
        public Guid RegistrationId { get; set; }
    }
}