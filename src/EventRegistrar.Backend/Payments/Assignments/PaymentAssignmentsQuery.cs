using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.Mediator;
using EventRegistrar.Backend.Payments.Assignments.Candidates;

namespace EventRegistrar.Backend.Payments.Assignments;

public class PaymentAssignmentsQuery : IRequest<SerializedJson<PaymentAssignments>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid PaymentId { get; set; }
}

public class PaymentAssignmentsQueryHandler : IRequestHandler<PaymentAssignmentsQuery, SerializedJson<PaymentAssignments>>
{
    private readonly ReadModelReader _readModelReader;

    public PaymentAssignmentsQueryHandler(ReadModelReader readModelReader)
    {
        _readModelReader = readModelReader;
    }

    public async Task<SerializedJson<PaymentAssignments>> Handle(PaymentAssignmentsQuery query,
                                                                 CancellationToken cancellationToken)
    {
        return await _readModelReader.Get<PaymentAssignments>(nameof(PaymentAssignmentsQuery),
                                                              query.EventId,
                                                              query.PaymentId,
                                                              cancellationToken);
    }
}