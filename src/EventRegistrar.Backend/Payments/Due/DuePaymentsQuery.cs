using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.Mediator;

namespace EventRegistrar.Backend.Payments.Due;

public class DuePaymentsQuery : IRequest<SerializedJson<IEnumerable<DuePaymentItem>>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class DuePaymentsQueryHandler : IRequestHandler<DuePaymentsQuery, SerializedJson<IEnumerable<DuePaymentItem>>>
{
    private readonly ReadModelReader _readModelReader;

    public DuePaymentsQueryHandler(ReadModelReader readModelReader)
    {
        _readModelReader = readModelReader;
    }

    public async Task<SerializedJson<IEnumerable<DuePaymentItem>>> Handle(DuePaymentsQuery query, CancellationToken cancellationToken)
    {
        return await _readModelReader.Get<IEnumerable<DuePaymentItem>>(nameof(DuePaymentsQuery),
                                                                       query.EventId,
                                                                       null,
                                                                       cancellationToken);
    }
}