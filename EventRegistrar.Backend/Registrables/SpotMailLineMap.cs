using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrables
{
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
}
