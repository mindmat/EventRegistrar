using System;
using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.Payments.Files
{
    public class PaymentFile : Entity
    {
        public string AccountIban { get; set; }
        public decimal? Balance { get; set; }
        public DateTime? BookingsFrom { get; set; }
        public DateTime? BookingsTo { get; set; }
        public string Content { get; set; }
        public string Currency { get; set; }
        public Guid? EventId { get; set; }
        public string FileId { get; set; }
    }
}