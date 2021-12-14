using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Payments.Files.Slips;

public class PaymentSlip : Entity
{
    public Guid EventId { get; set; }
    public Event? Event { get; set; }

    public string? ContentType { get; set; }
    public byte[]? FileBinary { get; set; }
    public string? Filename { get; set; }
    public string? Reference { get; set; }
}

public class PaymentSlipMap : EntityMap<PaymentSlip>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PaymentSlip> builder)
    {
        builder.ToTable("PaymentSlips");

        builder.HasOne(psl => psl.Event)
               .WithMany()
               .HasForeignKey(psl => psl.EventId);

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