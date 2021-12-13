using EventRegistrar.Backend.Infrastructure.DataAccess;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Mailing.Feedback;

public class RawMailEvent : Entity
{
    public string Body { get; set; } = null!;
    public DateTime Created { get; set; }
    public DateTime? Processed { get; set; }
}

public class RawMailEventsMap : EntityMap<RawMailEvent>
{
    protected override void ConfigureEntity(EntityTypeBuilder<RawMailEvent> builder)
    {
        builder.ToTable("RawMailEvents");
    }
}