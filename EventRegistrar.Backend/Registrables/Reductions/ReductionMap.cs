using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrables.Reductions
{
    public class ReductionMap : EntityTypeConfiguration<Reduction>
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
}