using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrables.Reductions;

public class Reduction : Entity
{
    [Obsolete("Use Registrable.ReducedPrice instead")]
    public bool ActivatedByReduction { get; set; }

    public decimal Amount { get; set; }

    public Role? OnlyForRole { get; set; }
    //public Guid? QuestionOptionId_ActivatesReduction { get; set; }

    public Guid RegistrableId { get; set; }
    public Registrable? Registrable { get; set; }

    public Guid? RegistrableId1_ReductionActivatedIfCombinedWith { get; set; }
    public Guid? RegistrableId2_ReductionActivatedIfCombinedWith { get; set; }
}

public class ReductionMap : EntityMap<Reduction>
{
    public override void Configure(EntityTypeBuilder<Reduction> builder)
    {
        base.Configure(builder);
        builder.ToTable("Reductions");
        builder.HasOne(red => red.Registrable)
               .WithMany(rbl => rbl.Reductions)
               .HasForeignKey(rsp => rsp.RegistrableId);
    }
}