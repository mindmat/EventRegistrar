using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Registrables;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Mailing.Templates;

public class MailTemplate : Entity
{
    public Guid EventId { get; set; }
    public Event? Event { get; set; }
    public Guid? RegistrableId { get; set; }
    public Registrable? Registrable { get; set; }
    public ICollection<Mail>? Mails { get; set; }

    public string? BulkMailKey { get; set; }
    public MailContentType ContentType { get; set; }
    public string? Language { get; set; }
    public MailingAudience? MailingAudience { get; set; }
    public string? SenderMail { get; set; }
    public string? SenderName { get; set; }
    public string? Subject { get; set; }
    public string? Template { get; set; }
    public MailType Type { get; set; }
    public bool IsDeleted { get; set; }
    public bool ReleaseImmediately { get; set; }
}

public class MailTemplateMap : EntityMap<MailTemplate>
{
    protected override void ConfigureEntity(EntityTypeBuilder<MailTemplate> builder)
    {
        builder.ToTable("MailTemplates");

        builder.HasOne(mtp => mtp.Event)
               .WithMany()
               .HasForeignKey(mtp => mtp.EventId);

        builder.HasOne(mtp => mtp.Registrable)
               .WithMany()
               .HasForeignKey(mtp => mtp.RegistrableId);
    }
}