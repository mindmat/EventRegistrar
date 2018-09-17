using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrations.IndividualReductions
{
    public class IndividualReductionMap : EntityTypeConfiguration<IndividualReduction>
    {
        public override void Configure(EntityTypeBuilder<IndividualReduction> builder)
        {
            base.Configure(builder);
            builder.ToTable("IndividualReductions");

            builder.HasOne(map => map.Registration)
                   .WithMany(mail => mail.IndividualReductions)
                   .HasForeignKey(map => map.RegistrationId);
        }
    }
}