using System;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Payments
{
    public class PaymentAssignment : Entity
    {
        public decimal Amount { get; set; }
        public DateTime? Created { get; set; }
        public Guid? PaymentAssignmentId_Counter { get; set; }
        public ReceivedPayment ReceivedPayment { get; set; }
        public Guid ReceivedPaymentId { get; set; }
        public Registration Registration { get; set; }
        public Guid RegistrationId { get; set; }
    }
}