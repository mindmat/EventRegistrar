using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Registrables.ReadModels;

using MediatR;

namespace EventRegistrar.Backend.Registrables;

public class RegistrablesOverviewQuery : IRequest<RegistrablesOverview>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class RegistrablesOverviewQueryHandler : IRequestHandler<RegistrablesOverviewQuery, RegistrablesOverview>
{
    private readonly IQueryable<RegistrablesOverviewQueryReadModel> _overviews;

    public RegistrablesOverviewQueryHandler(IQueryable<RegistrablesOverviewQueryReadModel> overviews)
    {
        _overviews = overviews;
    }

    public async Task<RegistrablesOverview> Handle(RegistrablesOverviewQuery query, CancellationToken cancellationToken)
    {
        var overview = await _overviews.Where(reg => reg.EventId == query.EventId)
                                       .Select(reg => reg.Content)
                                       .FirstAsync(cancellationToken);
        return overview;
    }
}

public class RegistrablesOverview
{
    public IEnumerable<SingleRegistrableDisplayItem> SingleRegistrables { get; set; }
    public IEnumerable<DoubleRegistrableDisplayItem> DoubleRegistrables { get; set; }
}