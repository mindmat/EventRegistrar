using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Payments.Files;

public class PaymentsFile : Entity
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

public class PaymentsFileMap : EntityMap<PaymentsFile>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PaymentsFile> builder)
    {
        builder.ToTable("PaymentsFiles");

        builder.HasOne(pmf => pmf.Event)
               .WithMany()
               .HasForeignKey(pmf => pmf.EventId);
    }
}