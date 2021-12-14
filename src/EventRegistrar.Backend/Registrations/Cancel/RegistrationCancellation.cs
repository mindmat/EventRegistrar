using EventRegistrar.Backend.Infrastructure.DataAccess;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrations.Cancel;

public class RegistrationCancellation : Entity
{
    public Guid RegistrationId { get; set; }
    public Registration? Registration { get; set; }

    public DateTime Created { get; set; }
    public string? Reason { get; set; }
    public decimal Refund { get; set; }
    public decimal RefundPercentage { get; set; }
    public DateTime? Received { get; set; }
}

public class RegistrationCancellationMap : EntityMap<RegistrationCancellation>
{
    protected override void ConfigureEntity(EntityTypeBuilder<RegistrationCancellation> builder)
    {
        builder.ToTable("RegistrationCancellations");

        builder.HasOne(rca => rca.Registration)
               .WithMany(reg => reg.Cancellations)
               .HasForeignKey(rca => rca.RegistrationId);
    }
}