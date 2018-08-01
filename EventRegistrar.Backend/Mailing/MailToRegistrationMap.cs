using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Mailing
{
    public class MailToRegistrationMap : EntityTypeConfiguration<MailToRegistration>
    {
        public override void Configure(EntityTypeBuilder<MailToRegistration> builder)
        {
            base.Configure(builder);
            builder.ToTable("MailToRegistrations");

            builder.HasOne(map => map.Mail)
                .WithMany(mail => mail.Registrations)
                .HasForeignKey(map => map.MailId);

            builder.HasOne(map => map.Registration)
                .WithMany(reg => reg.Mails)
                .HasForeignKey(map => map.RegistrationId);
        }
    }
}