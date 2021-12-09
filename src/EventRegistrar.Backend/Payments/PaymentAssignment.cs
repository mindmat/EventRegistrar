using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Payments.Refunds;
using EventRegistrar.Backend.Registrations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Payments;

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

public class PaymentAssignmentMap : EntityTypeConfiguration<PaymentAssignment>
{
    public override void Configure(EntityTypeBuilder<PaymentAssignment> builder)
    {
        base.Configure(builder);
        builder.ToTable("PaymentAssignments");

        builder.HasOne(pas => pas.Payment)
               .WithMany(pmt => pmt!.Assignments)
               .HasForeignKey(pas => pas.ReceivedPaymentId);

        builder.HasOne(pas => pas.Repayment)
               .WithMany(pmt => pmt!.RepaymentAssignments)
               .HasForeignKey(pas => pas.PaymentId_Repayment);

        builder.HasOne(pas => pas.Registration)
               .WithMany(pmt => pmt!.Payments)
               .HasForeignKey(pas => pas.RegistrationId);

        builder.HasOne(pas => pas.PayoutRequest)
               .WithMany(pmt => pmt!.Assignments)
               .HasForeignKey(pas => pas.PayoutRequestId);
    }
}