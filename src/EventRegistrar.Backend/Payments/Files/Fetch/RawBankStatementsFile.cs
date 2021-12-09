using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Payments.Files.Fetch;

public class RawBankStatementsFile : Entity
{
    public string? Server { get; set; }
    public string? ContractIdentifier { get; set; }
    public string? Filename { get; set; }
    public byte[]? Content { get; set; }
    public DateTimeOffset? Imported { get; set; }
    public DateTimeOffset? Processed { get; set; }
}

public class RawBankStatementsFileMap : EntityTypeConfiguration<RawBankStatementsFile>
{
    public override void Configure(EntityTypeBuilder<RawBankStatementsFile> builder)
    {
        base.Configure(builder);
        builder.ToTable("RawBankStatementsFiles");

        builder.Property(bst => bst.Server)
               .HasMaxLength(200);

        builder.Property(bst => bst.ContractIdentifier)
               .HasMaxLength(100);
    }
}