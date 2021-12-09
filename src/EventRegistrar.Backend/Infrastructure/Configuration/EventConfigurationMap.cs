using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Infrastructure.Configuration;

public class EventConfigurationMap : EntityTypeConfiguration<EventConfiguration>
{
    public override void Configure(EntityTypeBuilder<EventConfiguration> builder)
    {
        base.Configure(builder);
        builder.HasOne(cfg => cfg.Event)
               .WithMany(evt => evt.Configurations)
               .HasForeignKey(cfg => cfg.EventId);
    }
}