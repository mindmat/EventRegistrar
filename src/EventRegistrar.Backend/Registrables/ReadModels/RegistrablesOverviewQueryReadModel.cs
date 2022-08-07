using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrables.ReadModels;

public class RegistrablesOverviewQueryReadModel : ReadModel<RegistrablesOverview>
{
    public Guid EventId { get; set; }
}

public class RegistrablesOverviewQueryReadModelMap : ReadModelMap<RegistrablesOverviewQueryReadModel, RegistrablesOverview>
{
    protected override void ConfigureEntity(EntityTypeBuilder<RegistrablesOverviewQueryReadModel> builder)
    {
        builder.HasKey(rgq => rgq.EventId)
               .IsClustered(false);
    }
}