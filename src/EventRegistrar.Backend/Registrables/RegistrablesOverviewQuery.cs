using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.Mediator;

using MediatR;

namespace EventRegistrar.Backend.Registrables;

public class RegistrablesOverviewQuery : IRequest<SerializedJson<RegistrablesOverview>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class RegistrablesOverviewQueryHandler : IRequestHandler<RegistrablesOverviewQuery, SerializedJson<RegistrablesOverview>>
{
    private readonly ReadModelReader _readModelReader;

    public RegistrablesOverviewQueryHandler(ReadModelReader readModelReader)
    {
        _readModelReader = readModelReader;
    }


    public async Task<SerializedJson<RegistrablesOverview>> Handle(RegistrablesOverviewQuery query, CancellationToken cancellationToken)
    {
        return await _readModelReader.Get<RegistrablesOverview>(nameof(RegistrablesOverviewQuery),
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