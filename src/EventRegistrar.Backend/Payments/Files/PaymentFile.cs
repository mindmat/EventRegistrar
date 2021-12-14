using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Payments.Files;

public class PaymentFile : Entity
{
    public Guid? EventId { get; set; }
    public Event? Event { get; set; }

    public string? AccountIban { get; set; }
    public string? FileId { get; set; }
    public decimal? Balance { get; set; }
    public DateTime? BookingsFrom { get; set; }
    public DateTime? BookingsTo { get; set; }
    public string? Currency { get; set; }
    public string? Content { get; set; }
}

public class PaymentFileMap : EntityMap<PaymentFile>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PaymentFile> builder)
    {
        builder.ToTable("PaymentFiles");

        builder.HasOne(psl => psl.Event)
               .WithMany()
               .HasForeignKey(psl => psl.EventId);
    }
}