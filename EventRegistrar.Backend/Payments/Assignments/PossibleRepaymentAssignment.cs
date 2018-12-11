using System;

namespace EventRegistrar.Backend.Payments.Assignments
{
    public class PossibleRepaymentAssignment
    {
        public decimal Amount { get; set; }
        public decimal AmountUnsettled { get; set; }
        public DateTime BookingDate { get; set; }
        public string Currency { get; set; }
        public string DebitorName { get; set; }
        public string Info { get; set; }
        public int MatchScore { get; set; }
        public Guid PaymentId_Counter { get; set; }
        public Guid PaymentId_OpenPosition { get; set; }
        public bool Settled { get; set; }
    }
}