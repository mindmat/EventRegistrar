using System;

using EventRegistrar.Backend.Infrastructure.DataAccess;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Mailing.Feedback
{
    public class MailEvent : Entity
    {
        public DateTime Created { get; set; }
        public string? EMail { get; set; }
        public string? ExternalIdentifier { get; set; }
        public Mail? Mail { get; set; }
        public Guid MailId { get; set; }
        public string? RawEvent { get; set; }
        public MailState State { get; set; }
    }

    public class MailEventMap : EntityTypeConfiguration<MailEvent>
    {
        public override void Configure(EntityTypeBuilder<MailEvent> builder)
        {
            builder.ToTable("MailEvents");

            builder.HasOne(mev => mev.Mail)
                   .WithMany(mail => mail!.Events)
                   .HasForeignKey(mev => mev.MailId);

            base.Configure(builder);
        }
    }
}