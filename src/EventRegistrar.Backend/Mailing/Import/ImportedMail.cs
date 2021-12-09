using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Mailing.Import;

public class ImportedMail : Entity
{
    public string? ContentHtml { get; set; }
    public string? ContentPlainText { get; set; }
    public DateTime Date { get; set; }
    public Guid EventId { get; set; }
    public DateTime Imported { get; set; }
    public string? MessageIdentifier { get; set; }
    public string? Recipients { get; set; }
    public ICollection<ImportedMailToRegistration>? Registrations { get; set; }
    public string? SenderMail { get; set; }
    public string? SenderName { get; set; }
    public string? SendGridMessageId { get; set; }
    public string? Subject { get; set; }
}

public class ImportedMailMap : EntityTypeConfiguration<ImportedMail>
{
    public override void Configure(EntityTypeBuilder<ImportedMail> builder)
    {
        base.Configure(builder);
        builder.ToTable("ImportedMails");

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