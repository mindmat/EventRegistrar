using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Mailing;

public class MailAttachment : Entity
{
    public Guid MailId { get; set; }
    public Mail? Mail { get; set; }
    public byte[] Content { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? ContentType { get; set; }
}

public class MailAttachmentMap : EntityMap<MailAttachment>
{
    protected override void ConfigureEntity(EntityTypeBuilder<MailAttachment> builder)
    {
        builder.ToTable("MailAttachments");

        builder.HasOne(mat => mat.Mail)
               .WithMany(mail => mail.Attachments)
               .HasForeignKey(mat => mat.MailId);

        builder.Property(mat => mat.Name)
               .HasMaxLength(200);
        builder.Property(mat => mat.ContentType)
               .HasMaxLength(100);
    }
}