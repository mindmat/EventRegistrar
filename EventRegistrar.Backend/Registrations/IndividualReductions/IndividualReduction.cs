using System;
using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.Registrations.IndividualReductions
{
    public class IndividualReduction : Entity
    {
        public decimal Amount { get; set; }
        public string Reason { get; set; }
        public Registration Registration { get; set; }
        public Guid RegistrationId { get; set; }
        public Guid UserId { get; set; }
    }
}