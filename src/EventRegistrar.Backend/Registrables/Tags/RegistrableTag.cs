using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrables.Tags;

public class RegistrableTag : Entity
{
    public Guid EventId { get; set; }
    public Event? Event { get; set; }
    public string Tag { get; set; } = null!;
    public string FallbackText { get; set; } = null!;
}

public class RegistrableTagMap : EntityMap<RegistrableTag>
{
    protected override void ConfigureEntity(EntityTypeBuilder<RegistrableTag> builder)
    {
        builder.ToTable("RegistrableTags");

        builder.HasOne(rbt => rbt.Event)
               .WithMany()
               .HasForeignKey(rbt => rbt.EventId);

        builder.Property(rbt => rbt.Tag)
               .HasMaxLength(200);
        builder.Property(rbt => rbt.FallbackText)
               .HasMaxLength(200);
    }
}