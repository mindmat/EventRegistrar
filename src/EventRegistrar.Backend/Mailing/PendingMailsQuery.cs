using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.Mediator;

namespace EventRegistrar.Backend.Mailing;

public class PendingMailsQuery : IRequest<SerializedJson<IEnumerable<PendingMailListItem>>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class PendingMailsQueryHandler(ReadModelReader readModelReader) : IRequestHandler<PendingMailsQuery, SerializedJson<IEnumerable<PendingMailListItem>>>
{
    public async Task<SerializedJson<IEnumerable<PendingMailListItem>>> Handle(PendingMailsQuery query, CancellationToken cancellationToken)
    {
        return await readModelReader.Get<IEnumerable<PendingMailListItem>>(nameof(PendingMailsQuery),
                                                                           query.EventId,
                                                                           null,
                                                                           cancellationToken);
    }
}