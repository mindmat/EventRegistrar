using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Payments.Files.Slips;

public class PaymentSlipMap : EntityMap<PaymentSlip>
{
    public override void Configure(EntityTypeBuilder<PaymentSlip> builder)
    {
        base.Configure(builder);
        builder.ToTable("PaymentSlips");

        builder.Property(psl => psl.Filename)
               .HasMaxLength(1000);

        builder.Property(psl => psl.Reference)
               .HasMaxLength(100);

        builder.Property(psl => psl.ContentType)
               .HasMaxLength(100);

        builder.HasOne(psl => psl.Event)
               .WithMany()
               .HasForeignKey(psl => psl.EventId);
    }
}