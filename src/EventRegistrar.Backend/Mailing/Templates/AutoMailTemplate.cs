using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Mailing.Templates;

public class AutoMailTemplate : Entity
{
    public Guid EventId { get; set; }
    public Event? Event { get; set; }

    public string Language { get; set; } = null!;
    public MailType Type { get; set; }
    public string? Subject { get; set; }
    public string? ContentHtml { get; set; }
    public bool ReleaseImmediately { get; set; }
    public ICollection<Mail>? Mails { get; set; }
}

public class AutoMailTemplateMap : EntityMap<AutoMailTemplate>
{
    protected override void ConfigureEntity(EntityTypeBuilder<AutoMailTemplate> builder)
    {
        builder.ToTable("AutoMailTemplates");

        builder.HasOne(mtp => mtp.Event)
               .WithMany(evt => evt.AutoMailTemplates)
               .HasForeignKey(mtp => mtp.EventId);

        builder.Property(mtp => mtp.Language)
               .HasMaxLength(10);

        builder.Property(mtp => mtp.Subject)
               .HasMaxLength(1000);
    }
}