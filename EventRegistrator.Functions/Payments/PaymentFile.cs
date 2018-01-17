using System;
using EventRegistrator.Functions.Infrastructure.DataAccess;

namespace EventRegistrator.Functions.Payments
{
    public class PaymentFile : Entity
    {
        public Guid? EventId { get; set; }
        public string Content { get; set; }
        public string FileId { get; set; }
        public string AccountIban { get; set; }
        public string Currency { get; set; }
        public decimal Balance { get; set; }
    }
}