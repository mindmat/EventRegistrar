using EventRegistrar.Backend.Events;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Mailing.Import;

public class ImportedMail : Entity
{
    public Guid EventId { get; set; }
    public Event? Event { get; set; }
    public ICollection<ImportedMailToRegistration>? Registrations { get; set; }
    public Guid? ExternalMailConfigurationId { get; set; }

    public string? SenderMail { get; set; }
    public string? SenderName { get; set; }
    public string? Subject { get; set; }
    public string? Recipients { get; set; }
    public string? ContentHtml { get; set; }
    public string? ContentPlainText { get; set; }
    public DateTimeOffset Date { get; set; }
    public DateTimeOffset Imported { get; set; }
    public string? MessageIdentifier { get; set; }
    public string? SendGridMessageId { get; set; }
}

public class ImportedMailMap : EntityMap<ImportedMail>
{
    protected override void ConfigureEntity(EntityTypeBuilder<ImportedMail> builder)
    {
        builder.ToTable("ImportedMails");

        builder.HasOne(iml => iml.Event)
               .WithMany()
               .HasForeignKey(iml => iml.EventId);

        builder.Property(iml => iml.SenderName)
               .HasMaxLength(200);
        builder.Property(iml => iml.SenderMail)
               .HasMaxLength(200);
        builder.Property(iml => iml.Recipients)
               .HasMaxLength(500);
        builder.Property(iml => iml.SendGridMessageId)
               .HasMaxLength(500);
        builder.Property(iml => iml.Subject)
               .HasMaxLength(500);
        builder.Property(iml => iml.MessageIdentifier)
               .HasMaxLength(500);
    }
}