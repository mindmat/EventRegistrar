using System;

namespace EventRegistrar.Backend.Payments
{
    public class BalanceDto
    {
        public string AccountIban { get; set; }
        public decimal? Balance { get; set; }
        public string Currency { get; set; }
        public DateTime? Date { get; set; }
    }
}