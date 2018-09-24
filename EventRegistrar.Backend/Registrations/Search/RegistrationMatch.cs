using System;

namespace EventRegistrar.Backend.Registrations.Search
{
    public class RegistrationMatch
    {
        public decimal AmountPaid { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal Amount { get; set; }
        public Guid RegistrationId { get; set; }
        public RegistrationState State { get; set; }
    }
}