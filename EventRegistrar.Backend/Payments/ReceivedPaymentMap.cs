using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Payments
{
    public class ReceivedPaymentMap : EntityTypeConfiguration<ReceivedPayment>
    {
        public override void Configure(EntityTypeBuilder<ReceivedPayment> builder)
        {
            base.Configure(builder);
            builder.ToTable("ReceivedPayments");

            builder.HasOne(pmt => pmt.PaymentFile)
                   .WithMany()
                   .HasForeignKey(pmt => pmt.PaymentFileId);
        }
    }
}