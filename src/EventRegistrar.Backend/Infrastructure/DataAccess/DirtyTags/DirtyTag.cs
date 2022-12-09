using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Infrastructure.DataAccess.DirtyTags;

public class DirtyTag
{
    public long Id { get; set; }
    public string Entity { get; set; } = null!;
    public Guid EntityId { get; set; }
    public string Segment { get; set; } = null!;
    public DateTimeOffset DirtyMoment { get; set; }
}

public class DirtyTagMap : IEntityTypeConfiguration<DirtyTag>
{
    public void Configure(EntityTypeBuilder<DirtyTag> builder)
    {
        builder.ToTable("DirtyTags");

        builder.HasKey(dty => dty.Id);
        builder.HasIndex(dty => new
                                {
                                    dty.Entity,
                                    dty.EntityId,
                                    dty.Segment
                                });
    }
}