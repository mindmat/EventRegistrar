using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrations.Raw;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.RegistrationForms.GoogleForms
{
    public class RawRegistrationFormMap : EntityTypeConfiguration<RawRegistration>
    {
        public override void Configure(EntityTypeBuilder<RawRegistration> builder)
        {
            base.Configure(builder);
            builder.ToTable("RawRegistrationForms");
        }
    }
}