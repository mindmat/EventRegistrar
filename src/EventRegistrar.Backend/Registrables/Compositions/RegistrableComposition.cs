using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrables.Compositions;

public class RegistrableComposition : Entity
{
    public Guid RegistrableId { get; set; }
    public Registrable? Registrable { get; set; }

    public Guid RegistrableId_Contains { get; set; }
    public Registrable? Registrable_Contains { get; set; }
}

public class RegistrableCompositionMap : EntityMap<RegistrableComposition>
{
    public override void Configure(EntityTypeBuilder<RegistrableComposition> builder)
    {
        base.Configure(builder);
        builder.ToTable("RegistrableCompositions");

        builder.HasOne(cmp => cmp.Registrable)
               .WithMany(rbl => rbl!.Compositions)
               .HasForeignKey(cmp => cmp.RegistrableId);

        builder.HasOne(cmp => cmp.Registrable_Contains)
               .WithMany()
               .HasForeignKey(cmp => cmp.RegistrableId_Contains);
    }
}