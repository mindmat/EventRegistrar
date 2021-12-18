using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Infrastructure.DomainEvents;

public class PersistedDomainEvent : Entity
{
    public Guid? EventId { get; set; }
    public Event? Event { get; set; }

    public string Type { get; set; } = null!;
    public string Data { get; set; } = null!;
    public Guid? DomainEventId_Parent { get; set; }
    public DateTime Timestamp { get; set; }
}

public class PersistedDomainEventMap : EntityMap<PersistedDomainEvent>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PersistedDomainEvent> builder)
    {
        builder.ToTable("DomainEvents");

        builder.HasOne(dev => dev.Event)
               .WithMany()
               .HasForeignKey(dev => dev.EventId);

        builder.Property(dev => dev.Type)
               .HasMaxLength(300);

        builder.HasIndex(dev => dev.Timestamp)
               .IsUnique(false);
    }
}