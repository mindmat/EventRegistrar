using System;
using EventRegistrator.Functions.Infrastructure.DataAccess;

namespace EventRegistrator.Functions.Payments
{
    public class ReceivedPayment : Entity
    {
        public Guid PaymentFileId { get; set; }
        public PaymentFile PaymentFile { get; set; }
        public Guid? RegistrationId_Payer { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime BookingDate { get; set; }
        public string Info { get; set; }
        public string RecognizedEmail { get; set; }
        public string Reference { get; set; }
        public bool Settled { get; set; }
    }
}