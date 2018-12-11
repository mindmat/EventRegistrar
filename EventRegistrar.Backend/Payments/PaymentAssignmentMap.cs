using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Payments
{
    public class PaymentAssignmentMap : EntityTypeConfiguration<PaymentAssignment>
    {
        public override void Configure(EntityTypeBuilder<PaymentAssignment> builder)
        {
            base.Configure(builder);
            builder.ToTable("PaymentAssignments");

            builder.HasOne(pas => pas.Payment)
                   .WithMany(pmt => pmt.Assignments)
                   .HasForeignKey(pas => pas.ReceivedPaymentId);

            builder.HasOne(pas => pas.Repayment)
                   .WithMany(pmt => pmt.RepaymentAssignments)
                   .HasForeignKey(pas => pas.PaymentId_Repayment);

            builder.HasOne(pas => pas.Registration)
                   .WithMany(pmt => pmt.Payments)
                   .HasForeignKey(pas => pas.RegistrationId);
        }
    }
}