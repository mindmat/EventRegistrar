using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrations.Cancel
{
    public class RegistrationCancellationMap : EntityTypeConfiguration<RegistrationCancellation>
    {
        public override void Configure(EntityTypeBuilder<RegistrationCancellation> builder)
        {
            base.Configure(builder);
            builder.ToTable("RegistrationCancellations");

            builder.HasOne(rca => rca.Registration)
                   .WithMany(reg => reg.Cancellations)
                   .HasForeignKey(rca => rca.RegistrationId);
        }
    }
}