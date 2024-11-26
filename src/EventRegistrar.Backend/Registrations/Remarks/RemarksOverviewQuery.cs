using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.Mediator;

namespace EventRegistrar.Backend.Registrations.Remarks;

public class RemarksOverviewQuery : IRequest<SerializedJson<IEnumerable<RemarksDisplayItem>>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class RemarksOverviewQueryHandler(ReadModelReader readModelReader) : IRequestHandler<RemarksOverviewQuery, SerializedJson<IEnumerable<RemarksDisplayItem>>>
{
    public async Task<SerializedJson<IEnumerable<RemarksDisplayItem>>> Handle(RemarksOverviewQuery query, CancellationToken cancellationToken)
    {
        return await readModelReader.Get<IEnumerable<RemarksDisplayItem>>(nameof(RemarksOverviewQuery),
                                                                          query.EventId,
                                                                          null,
                                                                          cancellationToken);
    }
}
