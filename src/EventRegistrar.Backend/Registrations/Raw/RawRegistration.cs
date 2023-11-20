using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrations.Raw;

public class RawRegistration : Entity
{
    public DateTimeOffset Created { get; set; }
    public string? EventAcronym { get; set; }
    public string? FormExternalIdentifier { get; set; }
    public string ReceivedMessage { get; set; } = null!;
    public string RegistrationExternalIdentifier { get; set; } = null!;
    public string? LastProcessingError { get; set; }
    public DateTimeOffset? Processed { get; set; }
}

public class RawRegistrationMap : EntityMap<RawRegistration>
{
    protected override void ConfigureEntity(EntityTypeBuilder<RawRegistration> builder)
    {
        builder.ToTable("RawRegistrations");

        builder.Property(rrg => rrg.EventAcronym)
               .HasMaxLength(20);
        builder.Property(rrg => rrg.FormExternalIdentifier)
               .HasMaxLength(500);
        builder.Property(rrg => rrg.RegistrationExternalIdentifier)
               .HasMaxLength(500);
    }
}