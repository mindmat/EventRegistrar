using System;

namespace EventRegistrar.Backend.Payments.Assignments
{
    public class PossibleAssignment
    {
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public string FirstName { get; set; }
        public bool IsWaitingList { get; set; }
        public string LastName { get; set; }
        public int MatchScore { get; set; }
        public Guid PaymentId { get; set; }
        public Guid RegistrationId { get; set; }
    }
}