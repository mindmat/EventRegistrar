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

            builder.HasOne(reg => reg.Event)
                   .WithMany()
                   .HasForeignKey(reg => reg.EventId);

            builder.Property(reg => reg.PartnerNormalized)
                   .HasMaxLength(200);

            builder.Property(reg => reg.PartnerOriginal)
                   .HasMaxLength(200);

            builder.Property(reg => reg.RespondentEmail)
                   .HasMaxLength(200);

            builder.Property(reg => reg.RespondentFirstName)
                   .HasMaxLength(100);

            builder.Property(reg => reg.RespondentLastName)
                   .HasMaxLength(100);

            builder.Property(reg => reg.ExternalIdentifier)
                   .HasMaxLength(100);

            builder.Property(reg => reg.Phone)
                   .HasMaxLength(50);

            builder.Property(reg => reg.PhoneNormalized)
                   .HasMaxLength(50);

            builder.Property(reg => reg.Language)
                   .HasMaxLength(2);
        }
    }
}