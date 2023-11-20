using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.Mediator;

namespace EventRegistrar.Backend.Payments.Due;

public class DuePaymentsQuery : IRequest<SerializedJson<IEnumerable<DuePaymentItem>>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class DuePaymentsQueryHandler(ReadModelReader readModelReader) : IRequestHandler<DuePaymentsQuery, SerializedJson<IEnumerable<DuePaymentItem>>>
{
    public async Task<SerializedJson<IEnumerable<DuePaymentItem>>> Handle(DuePaymentsQuery query, CancellationToken cancellationToken)
    {
        return await readModelReader.Get<IEnumerable<DuePaymentItem>>(nameof(DuePaymentsQuery),
                                                                      query.EventId,
                                                                      null,
                                                                      cancellationToken);
    }
}