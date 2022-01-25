using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Payments.Files;

public class BankAccountStatementsFile : Entity
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

public class BankAccountStatementsFileMap : EntityMap<BankAccountStatementsFile>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BankAccountStatementsFile> builder)
    {
        builder.ToTable("BankAccountStatementsFiles");

        builder.HasOne(bbf => bbf.Event)
               .WithMany()
               .HasForeignKey(bbf => bbf.EventId);
    }
}