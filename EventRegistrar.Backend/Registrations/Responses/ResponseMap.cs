using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrations.Responses
{
    public class ResponseMap : EntityTypeConfiguration<Response>
    {
        public override void Configure(EntityTypeBuilder<Response> builder)
        {
            base.Configure(builder);
            builder.ToTable("Responses");

            builder.HasOne(rsp => rsp.Question)
                   .WithMany()
                   .HasForeignKey(rsp => rsp.QuestionId);
            builder.HasOne(rsp => rsp.Registration)
                   .WithMany()
                   .HasForeignKey(rsp => rsp.RegistrationId);
        }
    }
}