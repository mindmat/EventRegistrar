using System;

using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Payments.Refunds;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Payments
{
    public class PaymentAssignment : Entity
    {
        public decimal Amount { get; set; }
        public DateTime? Created { get; set; }
        public Guid? PaymentAssignmentId_Counter { get; set; }

        public Guid ReceivedPaymentId { get; set; }
        public ReceivedPayment? Payment { get; set; }

        public Guid? RegistrationId { get; set; }
        public Registration? Registration { get; set; }

        public Guid? PaymentId_Repayment { get; set; }
        public ReceivedPayment? Repayment { get; set; }

        public Guid? PayoutRequestId { get; set; }
        public PayoutRequest? PayoutRequest { get; set; }

    }
}