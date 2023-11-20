using EventRegistrar.Backend.Registrations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Mailing.Import;

public class ImportedMailToRegistration : Entity
{
    public Guid ImportedMailId { get; set; }
    public ImportedMail? Mail { get; set; }
    public Guid RegistrationId { get; set; }
    public Registration? Registration { get; set; }
}

public class ImportedMailToRegistrationMap : EntityMap<ImportedMailToRegistration>
{
    protected override void ConfigureEntity(EntityTypeBuilder<ImportedMailToRegistration> builder)
    {
        builder.ToTable("ImportedMailsToRegistrations");

        builder.HasOne(map => map.Mail)
               .WithMany(mail => mail.Registrations)
               .HasForeignKey(map => map.ImportedMailId);

        builder.HasOne(map => map.Registration)
               .WithMany(reg => reg.ImportedMails)
               .HasForeignKey(map => map.RegistrationId);
    }
}