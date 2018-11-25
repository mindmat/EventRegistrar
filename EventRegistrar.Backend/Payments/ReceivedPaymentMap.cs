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

            builder.Property(pmt => pmt.Info)
                   .HasMaxLength(400);

            builder.Property(pmt => pmt.Reference)
                   .HasMaxLength(100);

            builder.Property(pmt => pmt.RecognizedEmail)
                   .HasMaxLength(100);

            builder.Property(pmt => pmt.DebitorName)
                   .HasMaxLength(200);

            builder.Property(pmt => pmt.DebitorIban)
                   .HasMaxLength(200);

            builder.Property(pmt => pmt.InstructionIdentification)
                   .HasMaxLength(200);

            builder.HasOne(pmt => pmt.PaymentFile)
                   .WithMany()
                   .HasForeignKey(pmt => pmt.PaymentFileId);
        }
    }
}