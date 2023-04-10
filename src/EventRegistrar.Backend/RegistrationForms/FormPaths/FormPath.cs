using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrations.Register;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.RegistrationForms.FormPaths;

public class FormPath : Entity
{
    public Guid RegistrationFormId { get; set; }
    public RegistrationForm? RegistrationForm { get; set; }

    public string? Description { get; set; }
    public FormPathType Type { get; set; }
    public string ConfigurationJson { get; set; } = null!;
    public SingleRegistrationProcessConfiguration? SingleConfiguration { get; set; }
    public PartnerRegistrationProcessConfiguration? PartnerConfiguration { get; set; }
}

public class FormPathMap : EntityMap<FormPath>
{
    protected override void ConfigureEntity(EntityTypeBuilder<FormPath> builder)
    {
        builder.ToTable("FormPaths");

        builder.HasOne(fpt => fpt.RegistrationForm)
               .WithMany(frm => frm.FormPaths)
               .HasForeignKey(fpt => fpt.RegistrationFormId);

        builder.Property(ral => ral.SingleConfiguration)
               .HasConversion(StorageConverters.JsonConverter<SingleRegistrationProcessConfiguration>());

        builder.Property(ral => ral.PartnerConfiguration)
               .HasConversion(StorageConverters.JsonConverter<PartnerRegistrationProcessConfiguration>());
    }
}