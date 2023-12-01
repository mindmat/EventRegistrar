using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Mailing.Bulk;
using EventRegistrar.Backend.Mailing.Feedback;
using EventRegistrar.Backend.Mailing.Templates;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Mailing;

public class Mail : Entity
{
    public Guid? EventId { get; set; }
    public Event? Event { get; set; }
    public Guid? AutoMailTemplateId { get; set; }

    public AutoMailTemplate? AutoMailTemplate { get; set; }
    public Guid? BulkMailTemplateId { get; set; }
    public BulkMailTemplate? BulkMailTemplate { get; set; }

    public ICollection<MailEvent>? Events { get; set; }
    public ICollection<MailToRegistration>? Registrations { get; set; }

    public string? SenderMail { get; set; }
    public string? SenderName { get; set; }
    public string? Subject { get; set; }
    public string? Recipients { get; set; }
    public string? ContentHtml { get; set; }
    public string? ContentPlainText { get; set; }

    public DateTimeOffset Created { get; set; }
    public DateTimeOffset? Sent { get; set; }
    public MailState? State { get; set; }
    public MailType? Type { get; set; }

    public MailSender? SentBy { get; set; }
    public string? MailSenderMessageId { get; set; }

    public bool Withhold { get; set; }
    public bool Discarded { get; set; }

    public string? BulkMailKey { get; set; }

    public string? DataTypeFullName { get; set; }
    public string? DataJson { get; set; }
}

public class MailMap : EntityMap<Mail>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Mail> builder)
    {
        builder.ToTable("Mails");

        builder.HasOne(map => map.Event)
               .WithMany()
               .HasForeignKey(map => map.EventId);

        builder.HasOne(map => map.AutoMailTemplate)
               .WithMany(mail => mail.Mails)
               .HasForeignKey(map => map.AutoMailTemplateId);
    }
}