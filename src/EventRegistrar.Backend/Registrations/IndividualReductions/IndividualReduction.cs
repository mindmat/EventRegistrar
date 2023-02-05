using EventRegistrar.Backend.Authentication.Users;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrations.IndividualReductions;

public class IndividualReduction : Entity
{
    public Guid RegistrationId { get; set; }
    public Registration? Registration { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public IndividualReductionType Type { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
}

public enum IndividualReductionType
{
    Reduction = 1,
    OverwritePrice = 2
}

public class IndividualReductionMap : EntityMap<IndividualReduction>
{
    protected override void ConfigureEntity(EntityTypeBuilder<IndividualReduction> builder)
    {
        builder.ToTable("IndividualReductions");

        builder.HasOne(idr => idr.Registration)
               .WithMany(reg => reg.IndividualReductions)
               .HasForeignKey(idr => idr.RegistrationId);

        builder.HasOne(idr => idr.User)
               .WithMany()
               .HasForeignKey(idr => idr.UserId);
    }
}