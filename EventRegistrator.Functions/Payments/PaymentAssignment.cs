using System;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Registrations;

namespace EventRegistrator.Functions.Payments
{
    public class PaymentAssignment : Entity
    {
        public Guid RegistrationId { get; set; }
        public Registration Registration { get; set; }
        public Guid ReceivedPaymentId { get; set; }
        public ReceivedPayment ReceivedPayment { get; set; }
        public decimal Amount { get; set; }
    }
}