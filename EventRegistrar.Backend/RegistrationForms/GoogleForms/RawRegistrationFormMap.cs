using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.RegistrationForms.GoogleForms
{
    public class RawRegistrationFormMap : EntityTypeConfiguration<RawRegistrationForm>
    {
        public override void Configure(EntityTypeBuilder<RawRegistrationForm> builder)
        {
            base.Configure(builder);
            builder.ToTable("RawRegistrationForms");
        }
    }
}