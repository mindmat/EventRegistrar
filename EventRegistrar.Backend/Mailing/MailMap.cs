using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Mailing
{
    public class MailMap : EntityTypeConfiguration<Mail>
    {
        public override void Configure(EntityTypeBuilder<Mail> builder)
        {
            base.Configure(builder);
            builder.ToTable("Mails");
        }
    }
}