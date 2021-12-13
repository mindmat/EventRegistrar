using EventRegistrar.Backend.Infrastructure.DataAccess;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Mailing.Feedback;

public class MailEvent : Entity
{
    public Mail? Mail { get; set; }
    public Guid MailId { get; set; }

    public DateTime Created { get; set; }
    public string? EMail { get; set; }
    public string? ExternalIdentifier { get; set; }
    public string? RawEvent { get; set; }
    public MailState State { get; set; }
}

public class MailEventMap : EntityMap<MailEvent>
{
    protected override void ConfigureEntity(EntityTypeBuilder<MailEvent> builder)
    {
        builder.ToTable("MailEvents");

        builder.HasOne(mev => mev.Mail)
               .WithMany(mail => mail!.Events)
               .HasForeignKey(mev => mev.MailId);

        builder.Property(mev => mev.EMail)
               .HasMaxLength(200);
        builder.Property(mev => mev.ExternalIdentifier)
               .HasMaxLength(200);
    }
}