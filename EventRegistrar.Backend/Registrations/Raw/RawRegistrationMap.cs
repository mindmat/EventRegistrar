using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrations.Raw
{
    public class RawRegistrationMap : EntityTypeConfiguration<RawRegistration>
    {
        public override void Configure(EntityTypeBuilder<RawRegistration> builder)
        {
            base.Configure(builder);
            builder.ToTable("RawRegistrations");
        }
    }
}