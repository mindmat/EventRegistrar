using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

namespace EventRegistrar.Backend.Mailing.InvalidAddresses;

public class NotReceivedMailsQuery : IRequest<IEnumerable<ProblematicEmail>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public string? SearchString { get; set; }
}

public class NotReceivedMailsQueryHandler(ReadModelReader readModelReader)
    : IRequestHandler<NotReceivedMailsQuery, IEnumerable<ProblematicEmail>>
{
    public async Task<IEnumerable<ProblematicEmail>> Handle(NotReceivedMailsQuery query,
                                                            CancellationToken cancellationToken)
    {
        const int maxCount = 100;
        var results = await readModelReader.GetDeserialized<IEnumerable<ProblematicEmail>>(nameof(NotReceivedMailsQuery),
                                                                                           query.EventId,
                                                                                           (Guid?)null,
                                                                                           cancellationToken);
        return results.WhereIf(query.SearchString != null,
                               ml => ml.Email.Contains(query.SearchString!))
                      .Take(maxCount);
    }
}