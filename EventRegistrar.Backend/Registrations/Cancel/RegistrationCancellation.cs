using System;
using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.Registrations.Cancel
{
    public class RegistrationCancellation : Entity
    {
        public DateTime Created { get; set; }
        public string Reason { get; set; }
        public decimal Refund { get; set; }
        public decimal RefundPercentage { get; set; }
        public Registration Registration { get; set; }
        public Guid RegistrationId { get; set; }
    }
}