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

            builder.HasOne(pas => pas.ReceivedPayment)
                   .WithMany(pmt => pmt.Assignments)
                   .HasForeignKey(pas => pas.ReceivedPaymentId);

            builder.HasOne(pas => pas.Registration)
                   .WithMany(pmt => pmt.Payments)
                   .HasForeignKey(pas => pas.RegistrationId);
        }
    }
}