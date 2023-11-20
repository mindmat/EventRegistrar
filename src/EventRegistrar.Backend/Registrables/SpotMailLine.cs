using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrables;

public class SpotMailLine : Entity
{
    public Guid RegistrableId { get; set; }
    public Registrable? Registrable { get; set; }

    public string? Language { get; set; }
    public string? Text { get; set; }
}

public class SpotMailLineMap : EntityMap<SpotMailLine>
{
    protected override void ConfigureEntity(EntityTypeBuilder<SpotMailLine> builder)
    {
        builder.ToTable("SpotMailLines");

        builder.HasOne(sml => sml.Registrable)
               .WithMany()
               .HasForeignKey(sml => sml.RegistrableId);

        builder.Property(sml => sml.Language)
               .HasMaxLength(10);
    }
}