using EventRegistrar.Backend.Events;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Infrastructure.ReadableIds;

public class ReadableIdSource : Entity
{
    public Guid EventId { get; set; }
    public Event? Event { get; set; }
    public string? Prefix { get; set; }
    public uint Next { get; set; }
}

public class ReadableIdSourceMap : EntityMap<ReadableIdSource>
{
    protected override void ConfigureEntity(EntityTypeBuilder<ReadableIdSource> builder)
    {
        builder.ToTable("ReadableIdSources");

        builder.HasOne(ris => ris.Event)
               .WithMany()
               .HasForeignKey(ris => ris.EventId);

        builder.Property(ris => ris.Prefix)
               .HasMaxLength(20);

        builder.HasIndex(ris => new
                                {
                                    ris.EventId,
                                    ris.Prefix
                                })
               .IsUnique();
    }
}