using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Payments.Assignments.Candidates;

using MediatR;

namespace EventRegistrar.Backend.Payments.Assignments;

public class PaymentAssignmentsQuery : IRequest<PaymentAssignments>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid PaymentId { get; set; }
}

public class PaymentAssignmentsQueryHandler : IRequestHandler<PaymentAssignmentsQuery, PaymentAssignments>
{
    private readonly IQueryable<PaymentAssignmentsReadModel> _readModels;

    public PaymentAssignmentsQueryHandler(IQueryable<PaymentAssignmentsReadModel> readModels)
    {
        _readModels = readModels;
    }

    public async Task<PaymentAssignments> Handle(PaymentAssignmentsQuery query,
                                                 CancellationToken cancellationToken)
    {
        return await _readModels.Where(pmt => pmt.EventId == query.EventId
                                           && pmt.PaymentId == query.PaymentId)
                                .Select(pmt => pmt.Content)
                                .FirstAsync(cancellationToken);
    }
}