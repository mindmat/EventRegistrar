using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Mailing.Feedback;

public class RawMailEvent : Entity
{
    public string Body { get; set; } = null!;
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset? Processed { get; set; }
}

public class RawMailEventsMap : EntityMap<RawMailEvent>
{
    protected override void ConfigureEntity(EntityTypeBuilder<RawMailEvent> builder)
    {
        builder.ToTable("RawMailEvents");
    }
}