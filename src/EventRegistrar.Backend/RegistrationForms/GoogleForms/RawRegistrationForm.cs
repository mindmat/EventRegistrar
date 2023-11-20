using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.RegistrationForms.GoogleForms;

public class RawRegistrationForm : Entity
{
    public string FormExternalIdentifier { get; set; } = null!;
    public string EventAcronym { get; set; } = null!;
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset? Processed { get; set; }
    public string ReceivedMessage { get; set; } = null!;
}

public class RawRegistrationFormMap : EntityMap<RawRegistrationForm>
{
    protected override void ConfigureEntity(EntityTypeBuilder<RawRegistrationForm> builder)
    {
        builder.ToTable("RawRegistrationForms");

        builder.Property(rrf => rrf.EventAcronym)
               .HasMaxLength(20);
        builder.Property(rrf => rrf.FormExternalIdentifier)
               .HasMaxLength(200);
    }
}