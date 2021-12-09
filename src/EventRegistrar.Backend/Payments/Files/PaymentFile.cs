using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Payments.Files;

public class PaymentFile : Entity
{
    public string? AccountIban { get; set; }
    public decimal? Balance { get; set; }
    public DateTime? BookingsFrom { get; set; }
    public DateTime? BookingsTo { get; set; }
    public string? Content { get; set; }
    public string? Currency { get; set; }
    public Guid? EventId { get; set; }
    public string? FileId { get; set; }
}

public class PaymentFileMap : EntityTypeConfiguration<PaymentFile>
{
    public override void Configure(EntityTypeBuilder<PaymentFile> builder)
    {
        base.Configure(builder);
        builder.ToTable("PaymentFiles");
    }
}