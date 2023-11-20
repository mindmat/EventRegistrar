using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.Mediator;

namespace EventRegistrar.Backend.Registrables;

public class RegistrablesOverviewQuery : IRequest<SerializedJson<RegistrablesOverview>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class RegistrablesOverviewQueryHandler(ReadModelReader readModelReader) : IRequestHandler<RegistrablesOverviewQuery, SerializedJson<RegistrablesOverview>>
{
    public async Task<SerializedJson<RegistrablesOverview>> Handle(RegistrablesOverviewQuery query, CancellationToken cancellationToken)
    {
        return await readModelReader.Get<RegistrablesOverview>(nameof(RegistrablesOverviewQuery),
                                                               query.EventId,
                                                               null,
                                                               cancellationToken);
    }
}

public class RegistrablesOverview
{
    public IEnumerable<SingleRegistrableDisplayItem> SingleRegistrables { get; set; } = null!;
    public IEnumerable<DoubleRegistrableDisplayItem> DoubleRegistrables { get; set; } = null!;
}