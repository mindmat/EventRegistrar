using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Payments.Refunds;
using EventRegistrar.Backend.Registrations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Payments;

public class PaymentAssignment : Entity
{
    public Guid? RegistrationId { get; set; }
    public Registration? Registration { get; set; }
    public Guid ReceivedPaymentId { get; set; }
    public BankAccountBooking? ReceivedPayment { get; set; }
    public Guid? PaymentAssignmentId_Counter { get; set; }
    public PaymentAssignment? PaymentAssignment_Counter { get; set; }
    public Guid? PaymentId_Repayment { get; set; }
    public BankAccountBooking? ReceivedPayment_Repayment { get; set; }
    public Guid? PayoutRequestId { get; set; }
    public PayoutRequest? PayoutRequest { get; set; }

    public decimal Amount { get; set; }
    public DateTime? Created { get; set; }
}

public class PaymentAssignmentMap : EntityMap<PaymentAssignment>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PaymentAssignment> builder)
    {
        builder.ToTable("PaymentAssignments");

        builder.HasOne(pas => pas.ReceivedPayment)
               .WithMany(pmt => pmt.Assignments)
               .HasForeignKey(pas => pas.ReceivedPaymentId);

        builder.HasOne(pas => pas.ReceivedPayment_Repayment)
               .WithMany(pmt => pmt.RepaymentAssignments)
               .HasForeignKey(pas => pas.PaymentId_Repayment);

        builder.HasOne(pas => pas.Registration)
               .WithMany(pmt => pmt.Payments)
               .HasForeignKey(pas => pas.RegistrationId);

        builder.HasOne(pas => pas.PayoutRequest)
               .WithMany(pmt => pmt.Assignments)
               .HasForeignKey(pas => pas.PayoutRequestId);

        builder.HasOne(pas => pas.PaymentAssignment_Counter)
               .WithMany()
               .HasForeignKey(pas => pas.PaymentAssignmentId_Counter);
    }
}