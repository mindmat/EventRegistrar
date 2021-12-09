using EventRegistrar.Backend.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.RegistrationForms.GoogleForms;

public class RawRegistrationForm : Entity
{
    public DateTime Created { get; set; }
    public string EventAcronym { get; set; } = null!;
    public string FormExternalIdentifier { get; set; } = null!;
    public bool Processed { get; set; }
    public string ReceivedMessage { get; set; } = null!;
}

public class RawRegistrationFormMap : EntityTypeConfiguration<RawRegistrationForm>
{
    public override void Configure(EntityTypeBuilder<RawRegistrationForm> builder)
    {
        base.Configure(builder);
        builder.ToTable("RawRegistrationForms");
    }
}