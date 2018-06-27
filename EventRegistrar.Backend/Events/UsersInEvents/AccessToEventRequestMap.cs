using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Events.UsersInEvents
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
        }
    }
}