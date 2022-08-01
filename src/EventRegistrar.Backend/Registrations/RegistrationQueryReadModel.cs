using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrations;

public class RegistrationQueryReadModel : ReadModel<RegistrationDisplayItem>
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class RegistrationQueryReadModelMap : ReadModelMap<RegistrationQueryReadModel, RegistrationDisplayItem>
{
    protected override void ConfigureEntity(EntityTypeBuilder<RegistrationQueryReadModel> builder)
    {
        builder.HasKey(rgq => new
                              {
                                  rgq.EventId,
                                  rgq.RegistrationId
                              })
               .IsClustered(false);
    }
}