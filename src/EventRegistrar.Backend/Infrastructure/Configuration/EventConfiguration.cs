using EventRegistrar.Backend.Events;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Infrastructure.Configuration;

public class EventConfiguration : Entity
{
    public Event? Event { get; set; }
    public Guid EventId { get; set; }

    public string Type { get; set; } = null!;
    public string ValueJson { get; set; } = null!;
}

public class EventConfigurationMap : EntityMap<EventConfiguration>
{
    protected override void ConfigureEntity(EntityTypeBuilder<EventConfiguration> builder)
    {
        builder.ToTable("EventConfigurations");

        builder.HasOne(cfg => cfg.Event)
               .WithMany(evt => evt.Configurations)
               .HasForeignKey(cfg => cfg.EventId);

        builder.Property(cfg => cfg.Type)
               .HasMaxLength(300);
    }
}