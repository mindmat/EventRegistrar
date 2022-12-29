﻿using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Mailing.Templates;
using EventRegistrar.Backend.Registrables;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Mailing.Bulk;

public class BulkMailTemplate : Entity
{
    public Guid EventId { get; set; }
    public Event? Event { get; set; }
    public Guid? RegistrableId { get; set; }
    public Registrable? Registrable { get; set; }

    public string BulkMailKey { get; set; } = null!;
    public string? Language { get; set; }
    public MailingAudience? MailingAudience { get; set; }
    public string? Subject { get; set; }
    public string? ContentHtml { get; set; }

    public ICollection<Mail>? Mails { get; set; }
}

public class BulkMailTemplateMap : EntityMap<BulkMailTemplate>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BulkMailTemplate> builder)
    {
        builder.ToTable("BulkMailTemplates");

        builder.HasOne(mtp => mtp.Event)
               .WithMany(evt => evt.BulkMailTemplates)
               .HasForeignKey(mtp => mtp.EventId);

        builder.HasOne(mtp => mtp.Registrable)
               .WithMany()
               .HasForeignKey(mtp => mtp.RegistrableId);

        builder.Property(mtp => mtp.Language)
               .HasMaxLength(10);

        builder.Property(mtp => mtp.Subject)
               .HasMaxLength(1000);
    }
}