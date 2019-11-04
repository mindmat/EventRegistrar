using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Events
{
    public class EventMap : EntityTypeConfiguration<Event>
    {
        public override void Configure(EntityTypeBuilder<Event> builder)
        {
            base.Configure(builder);
            builder.ToTable("Events");

            builder.HasOne(evt => evt.PredecessorEvent)
                   .WithMany()
                   .HasForeignKey(evt => evt.PredecessorEventId);
        }
    }
}