using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrables
{
    public class RegistrableMap : EntityTypeConfiguration<Registrable>
    {
        public override void Configure(EntityTypeBuilder<Registrable> builder)
        {
            builder.ToTable("Registrables");
            base.Configure(builder);

            builder.HasOne(rbl => rbl.Event)
                   .WithMany(evt => evt.Registrables)
                   .HasForeignKey(rbl => rbl.EventId);
        }
    }
}