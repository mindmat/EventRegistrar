using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrations.IndividualReductions;

public class IndividualReduction : Entity
{
    public decimal Amount { get; set; }
    public string? Reason { get; set; }

    public Guid RegistrationId { get; set; }
    public Registration? Registration { get; set; }
    public Guid UserId { get; set; }
}

public class IndividualReductionMap : EntityTypeConfiguration<IndividualReduction>
{
    public override void Configure(EntityTypeBuilder<IndividualReduction> builder)
    {
        base.Configure(builder);
        builder.ToTable("IndividualReductions");

        builder.HasOne(map => map.Registration)
               .WithMany(mail => mail!.IndividualReductions)
               .HasForeignKey(map => map.RegistrationId);
    }
}