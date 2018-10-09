using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Mailing.Feedback
{
    public class MailEventMap : EntityTypeConfiguration<MailEvent>
    {
        public override void Configure(EntityTypeBuilder<MailEvent> builder)
        {
            builder.ToTable("MailEvents");
            base.Configure(builder);
        }
    }
}