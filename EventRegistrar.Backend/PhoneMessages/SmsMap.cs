using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.PhoneMessages
{
    public class SmsMap : EntityTypeConfiguration<Sms>
    {
        public override void Configure(EntityTypeBuilder<Sms> builder)
        {
            base.Configure(builder);

            builder.HasOne(sms => sms.Registration)
                   .WithMany(reg => reg.Sms)
                   .HasForeignKey(sms => sms.RegistrationId);
        }
    }
}