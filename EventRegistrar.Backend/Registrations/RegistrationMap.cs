using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrations
{
    public class RegistrationMap : EntityTypeConfiguration<Registration>
    {
        public override void Configure(EntityTypeBuilder<Registration> builder)
        {
            base.Configure(builder);
            builder.ToTable("Registrations");
            builder.HasOne(reg => reg.RegistrationForm)
                   .WithMany()
                   .HasForeignKey(reg => reg.RegistrationFormId);
        }
    }
}