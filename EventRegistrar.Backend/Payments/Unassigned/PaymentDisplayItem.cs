using System;

namespace EventRegistrar.Backend.Payments.Unassigned
{
    public class PaymentDisplayItem
    {
        public decimal Amount { get; set; }
        public decimal AmountAssigned { get; set; }
        public DateTime BookingDate { get; set; }
        public string Currency { get; set; }
        public Guid Id { get; set; }
        public string Info { get; set; }
        public Guid? PaymentSlipId { get; set; }
        public string Reference { get; set; }
        public decimal? AmountRepaid { get; set; }
        public bool Settled { get; set; }
        public bool Ignore { get; set; }
        public string Message { get; set; }
        public string DebitorName { get; set; }
    }
}