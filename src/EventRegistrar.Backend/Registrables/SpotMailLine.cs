using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrables;

public class SpotMailLine : Entity
{
    public Guid RegistrableId { get; set; }
    public string? Language { get; set; }
    public string? Text { get; set; }
}

public class SpotMailLineMap : EntityTypeConfiguration<SpotMailLine>
{
    public override void Configure(EntityTypeBuilder<SpotMailLine> builder)
    {
        builder.ToTable("SpotMailLines");
        builder.Property(sml => sml.Language)
               .HasMaxLength(10);
        base.Configure(builder);
    }
}