using EventRegistrar.Backend.Events;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Statistics;

/// <summary>
/// Entity to store periodic statistics data points.
/// Only essential data is stored: Key, EventId, CreatedAt, and Value.
/// All metadata is read from the respective calculator at query time.
/// </summary>
public class StatisticSnapshot : Entity
{
    /// <summary>
    /// Name identifying the statistic calculation.
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// Event ID for event-bound statistics, null for global statistics.
    /// </summary>
    public Guid? EventId { get; set; }

    /// <summary>
    /// Navigation property to event (if event-bound).
    /// </summary>
    public Event? Event { get; set; }

    /// <summary>
    /// When this snapshot was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// The numeric value of the statistic.
    /// </summary>
    public decimal Value { get; set; }
}

public class StatisticSnapshotMap : EntityMap<StatisticSnapshot>
{
    protected override void ConfigureEntity(EntityTypeBuilder<StatisticSnapshot> builder)
    {
        builder.ToTable("StatisticSnapshots");

        builder.Property(sts => sts.Key)
               .HasMaxLength(200)
               .IsRequired();

        builder.HasOne(sts => sts.Event)
               .WithMany()
               .HasForeignKey(sts => sts.EventId)
               .OnDelete(DeleteBehavior.Cascade);

        // Index for efficient querying
        builder.HasIndex(sts => new { sts.Key, sts.EventId, sts.CreatedAt });
    }
}