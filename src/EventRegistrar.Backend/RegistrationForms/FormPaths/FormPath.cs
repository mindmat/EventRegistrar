using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrations.Register;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

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

public class FormPathMap : EntityTypeConfiguration<FormPath>
{
    public override void Configure(EntityTypeBuilder<FormPath> builder)
    {
        base.Configure(builder);
        builder.ToTable("FormPaths");

        builder.HasOne(fpt => fpt.RegistrationForm)
               .WithMany(frm => frm!.FormPaths)
               .HasForeignKey(fpt => fpt.RegistrationFormId);

        builder.Property(ral => ral.SingleConfiguration)
               .HasConversion(StorageConverters.JsonConverter<SingleRegistrationProcessConfiguration>());

        builder.Property(ral => ral.PartnerConfiguration)
               .HasConversion(StorageConverters.JsonConverter<PartnerRegistrationProcessConfiguration>());
    }
}

public static class StorageConverters
{
    private static readonly JsonSerializerSettings _settings = new()
                                                               {
                                                                   DefaultValueHandling = DefaultValueHandling.Ignore,
                                                                   TypeNameHandling = TypeNameHandling.Auto
                                                               };

    public static ValueConverter<T?, string?> JsonConverter<T>()
        where T : class
    {
        return new ValueConverter<T?, string?>(
            value => value == null ? null : JsonConvert.SerializeObject(value, _settings),
            json => json == null ? null : JsonConvert.DeserializeObject<T>(json, _settings));
    }
}