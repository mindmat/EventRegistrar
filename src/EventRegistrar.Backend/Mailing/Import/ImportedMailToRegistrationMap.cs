using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Mailing.Import
{
    public class ImportedMailToRegistrationMap : EntityTypeConfiguration<ImportedMailToRegistration>
    {
        public override void Configure(EntityTypeBuilder<ImportedMailToRegistration> builder)
        {
            base.Configure(builder);
            builder.ToTable("ImportedMailToRegistrations");

            builder.HasOne(map => map.Mail)
                .WithMany(mail => mail.Registrations)
                .HasForeignKey(map => map.ImportedMailId);

            builder.HasOne(map => map.Registration)
                .WithMany(reg => reg.ImportedMails)
                .HasForeignKey(map => map.RegistrationId);
        }
    }
}