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

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Mail { get; set; }
    public string? Phone { get; set; }
    public bool RoleMissing { get; set; }
    public Role? RoleOverride { get; set; }
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
        builder.Property(rrg => rrg.FirstName)
               .HasMaxLength(100);
        builder.Property(rrg => rrg.LastName)
               .HasMaxLength(100);
        builder.Property(rrg => rrg.Mail)
               .HasMaxLength(200);
        builder.Property(rrg => rrg.Phone)
               .HasMaxLength(100);

        builder.HasIndex(rrg => rrg.EventAcronym); // Performance
    }
}