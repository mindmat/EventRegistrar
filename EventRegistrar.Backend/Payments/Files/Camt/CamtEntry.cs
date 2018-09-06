using System;

namespace EventRegistrar.Backend.Payments.Files.Camt
{
    public class CamtEntry
    {
        public decimal Amount { get; set; }
        public DateTime BookingDate { get; set; }
        public string Currency { get; set; }
        public string Info { get; set; }
        public string Reference { get; set; }
        public CreditDebit Type { get; set; }
    }
}