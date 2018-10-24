using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrables.Compositions
{
    public class RegistrableCompositionMap : EntityTypeConfiguration<RegistrableComposition>
    {
        public override void Configure(EntityTypeBuilder<RegistrableComposition> builder)
        {
            base.Configure(builder);
            builder.ToTable("RegistrableCompositions");

            builder.HasOne(cmp => cmp.Registrable)
                   .WithMany(rbl => rbl.Compositions)
                   .HasForeignKey(cmp => cmp.RegistrableId);

            builder.HasOne(cmp => cmp.Registrable_Contains)
                   .WithMany()
                   .HasForeignKey(cmp => cmp.RegistrableId_Contains);
        }
    }
}