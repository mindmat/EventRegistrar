using System;

using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.PhoneMessages
{
    public class Sms : Entity
    {
        public string? AccountSid { get; set; }
        public string? Body { get; set; }
        public string? Error { get; set; }
        public int? ErrorCode { get; set; }
        public string? From { get; set; }
        public string? Price { get; set; }
        public string? RawData { get; set; }
        public DateTime? Received { get; set; }
        public DateTime? Sent { get; set; }
        public string? SmsSid { get; set; }
        public string? SmsStatus { get; set; }
        public string? To { get; set; }
        public SmsType Type { get; set; }

        public Guid? RegistrationId { get; set; }
        public Registration? Registration { get; set; }

    }

    public class SmsMap : EntityTypeConfiguration<Sms>
    {
        public override void Configure(EntityTypeBuilder<Sms> builder)
        {
            base.Configure(builder);

            builder.HasOne(sms => sms.Registration)
                   .WithMany(reg => reg!.Sms)
                   .HasForeignKey(sms => sms.RegistrationId);
        }
    }
}