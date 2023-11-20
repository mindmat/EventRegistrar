using EventRegistrar.Backend.Registrations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrables.Reductions;

[Obsolete("Use PricingPackages instead")]
public class Reduction : Entity
{
    public Guid RegistrableId { get; set; }
    public Registrable? Registrable { get; set; }
    public Guid? RegistrableId1_ReductionActivatedIfCombinedWith { get; set; }
    public Registrable? Registrable1_ReductionActivatedIfCombinedWith { get; set; }
    public Guid? RegistrableId2_ReductionActivatedIfCombinedWith { get; set; }
    public Registrable? Registrable2_ReductionActivatedIfCombinedWith { get; set; }

    public decimal Amount { get; set; }
    public Role? OnlyForRole { get; set; }
    public bool ActivatedByReduction { get; set; }
}

public class ReductionMap : EntityMap<Reduction>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Reduction> builder)
    {
        builder.ToTable("Reductions");

        builder.HasOne(red => red.Registrable)
               .WithMany(rbl => rbl.Reductions)
               .HasForeignKey(rsp => rsp.RegistrableId);

        builder.HasOne(red => red.Registrable1_ReductionActivatedIfCombinedWith)
               .WithMany()
               .HasForeignKey(rsp => rsp.RegistrableId1_ReductionActivatedIfCombinedWith);

        builder.HasOne(red => red.Registrable2_ReductionActivatedIfCombinedWith)
               .WithMany()
               .HasForeignKey(rsp => rsp.RegistrableId2_ReductionActivatedIfCombinedWith);
    }
}