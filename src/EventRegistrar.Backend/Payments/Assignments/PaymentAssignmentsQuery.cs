using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Payments.Assignments.Candidates;

namespace EventRegistrar.Backend.Payments.Assignments;

public class PaymentAssignmentsQuery : IRequest<PaymentAssignments>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid PaymentId { get; set; }
}

public class PaymentAssignmentsQueryHandler : IRequestHandler<PaymentAssignmentsQuery, PaymentAssignments>
{
    private readonly IEnumerable<IReadModelCalculator> _calculators;

    public PaymentAssignmentsQueryHandler(IEnumerable<IReadModelCalculator> calculators)
    {
        _calculators = calculators;
    }

    public async Task<PaymentAssignments> Handle(PaymentAssignmentsQuery query,
                                                 CancellationToken cancellationToken)
    {
        var calculator = _calculators.First(rmc => rmc.QueryName == nameof(PaymentAssignmentsQuery));
        var assignments = await calculator.Calculate(query.EventId, query.PaymentId, cancellationToken);
        return (PaymentAssignments)assignments;
    }
}