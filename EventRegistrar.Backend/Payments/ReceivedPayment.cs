using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Payments.Files.Camt;
using EventRegistrar.Backend.Payments.Files.Slips;

namespace EventRegistrar.Backend.Payments
{
    public class ReceivedPayment : Entity
    {
        public decimal Amount { get; set; }
        public ICollection<PaymentAssignment> Assignments { get; set; }
        public DateTime BookingDate { get; set; }
        public decimal? Charges { get; set; }
        public CreditDebit? CreditDebitType { get; set; }
        public string Currency { get; set; }
        public string DebitorIban { get; set; }
        public string DebitorName { get; set; }
        public string Info { get; set; }
        public string InstructionIdentification { get; set; }
        public PaymentFile PaymentFile { get; set; }
        public Guid PaymentFileId { get; set; }
        public PaymentSlip PaymentSlip { get; set; }
        public Guid? PaymentSlipId { get; set; }
        public string RawXml { get; set; }
        public string RecognizedEmail { get; set; }
        public string Reference { get; set; }
        public Guid? RegistrationId_Payer { get; set; }
        public decimal? Repaid { get; set; }
        public ICollection<PaymentAssignment> RepaymentAssignments { get; set; }
        public bool Settled { get; set; }
    }
}