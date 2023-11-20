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
    protected override void ConfigureEntity(EntityTypeBuilder<RegistrableComposition> builder)
    {
        builder.ToTable("RegistrableCompositions");

        builder.HasOne(cmp => cmp.Registrable)
               .WithMany(rbl => rbl.Compositions)
               .HasForeignKey(cmp => cmp.RegistrableId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cmp => cmp.Registrable_Contains)
               .WithMany()
               .HasForeignKey(cmp => cmp.RegistrableId_Contains)
               .OnDelete(DeleteBehavior.Restrict);
    }
}