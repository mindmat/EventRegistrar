using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Mailing.Feedback;
using EventRegistrar.Backend.Mailing.Templates;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Mailing;

public class Mail : Entity
{
    public string? BulkMailKey { get; set; }
    public string? ContentHtml { get; set; }
    public string? ContentPlainText { get; set; }
    public DateTime Created { get; set; }
    public bool Discarded { get; set; }
    public Guid? EventId { get; set; }
    public ICollection<MailEvent>? Events { get; set; }
    public Guid? MailTemplateId { get; set; }
    public MailTemplate? MailTemplate { get; set; }
    public string? Recipients { get; set; }
    public ICollection<MailToRegistration>? Registrations { get; set; }
    public string? SenderMail { get; set; }
    public string? SenderName { get; set; }
    public string? SendGridMessageId { get; set; }
    public DateTime? Sent { get; set; }
    public MailState? State { get; set; }
    public string? Subject { get; set; }
    public MailType? Type { get; set; }
    public bool Withhold { get; set; }
    public string? DataTypeFullName { get; set; }
    public string? DataJson { get; set; }
}

public class MailMap : EntityTypeConfiguration<Mail>
{
    public override void Configure(EntityTypeBuilder<Mail> builder)
    {
        base.Configure(builder);
        builder.ToTable("Mails");

        builder.HasOne(map => map.MailTemplate)
               .WithMany(mail => mail.Mails)
               .HasForeignKey(map => map.MailTemplateId);
    }
}