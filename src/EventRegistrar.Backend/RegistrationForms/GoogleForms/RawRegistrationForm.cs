using Microsoft.EntityFrameworkCore.Metadata.Builders;

using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.RegistrationForms.GoogleForms;

public class RawRegistrationForm : Entity
{
    public DateTimeOffset Created { get; set; }
    public string EventAcronym { get; set; } = null!;
    public string FormExternalIdentifier { get; set; } = null!;
    public bool Processed { get; set; }
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