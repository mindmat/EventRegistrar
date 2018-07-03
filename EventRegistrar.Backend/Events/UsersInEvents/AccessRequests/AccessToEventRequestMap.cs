using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Events.UsersInEvents.AccessRequests
{
    public class AccessToEventRequestMap : EntityTypeConfiguration<AccessToEventRequest>
    {
        public override void Configure(EntityTypeBuilder<AccessToEventRequest> builder)
        {
            base.Configure(builder);
            builder.ToTable("AccessToEventRequests");

            builder.HasOne(arq => arq.Event)
                   .WithMany(evt => evt.AccessRequests)
                   .HasForeignKey(arq => arq.EventId);

            builder.Property(arq => arq.Identifier)
                .HasMaxLength(200);
            builder.Property(arq => arq.FirstName)
                .HasMaxLength(200);
            builder.Property(arq => arq.LastName)
                .HasMaxLength(200);
            builder.Property(arq => arq.Email)
                .HasMaxLength(200);
        }
    }
}