using System;

namespace EventRegistrar.Backend.Payments.Unrecognized
{
    public class PaymentDisplayItem
    {
        public decimal Amount { get; set; }
        public decimal? Assigned { get; set; }
        public DateTime BookingDate { get; set; }
        public string Currency { get; set; }
        public Guid Id { get; set; }
        public string Info { get; set; }
        public string Reference { get; set; }
        public decimal? Repaid { get; set; }
        public bool Settled { get; set; }
    }
}