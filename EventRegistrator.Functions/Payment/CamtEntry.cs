using System;

namespace EventRegistrator.Functions.Payment
{
    public class CamtEntry
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Info { get; set; }
        public CreditDebit Type { get; set; }
        public DateTime BookgDt { get; set; }
        public string Reference { get; set; }
    }
}