using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Mailing.Import
{
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
}