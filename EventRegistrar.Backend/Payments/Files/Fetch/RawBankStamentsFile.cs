using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Payments.Files.Fetch
{
    public class RawBankStamentsFile : Entity
    {
        public string Server { get; set; }
        public string ContractIdentifier { get; set; }
        public string Filename { get; set; }
        public byte[] Content { get; set; }
    }

    public class RawBankStamentsFileMap : EntityTypeConfiguration<RawBankStamentsFile>
    {
        public override void Configure(EntityTypeBuilder<RawBankStamentsFile> builder)
        {
            base.Configure(builder);
            builder.ToTable("RawBankStatmentsFiles");

            builder.Property(bst => bst.Server)
                   .HasMaxLength(200);

            builder.Property(bst => bst.ContractIdentifier)
                   .HasMaxLength(100);
        }
    }
}
