using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Mailing.Templates
{
    public class MailTemplateMap : EntityTypeConfiguration<MailTemplate>
    {
        public override void Configure(EntityTypeBuilder<MailTemplate> builder)
        {
            base.Configure(builder);
            builder.ToTable("MailTemplates");
        }
    }
}