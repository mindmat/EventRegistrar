using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Payments.Unassigned;
using MediatR;

namespace EventRegistrar.Backend.Payments.Statements;

public class PaymentStatementsQuery : IRequest<IEnumerable<PaymentDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class PaymentStatementsQueryHandler : IRequestHandler<PaymentStatementsQuery, IEnumerable<PaymentDisplayItem>>
{
    private readonly IQueryable<ReceivedPayment> _payments;

    public PaymentStatementsQueryHandler(IQueryable<ReceivedPayment> payments)
    {
        _payments = payments;
    }

    public async Task<IEnumerable<PaymentDisplayItem>> Handle(PaymentStatementsQuery query,
                                                              CancellationToken cancellationToken)
    {
        var payments = await _payments
                             .Where(rpy => rpy.PaymentFile.EventId == query.EventId)
                             .Select(rpy => new PaymentDisplayItem
                                            {
                                                Id = rpy.Id,
                                                Amount = rpy.Amount,
                                                AmountAssigned = rpy.Assignments.Sum(asn =>
                                                    asn.PayoutRequestId == null ? asn.Amount : -asn.Amount),
                                                BookingDate = rpy.BookingDate,
                                                Currency = rpy.Currency,
                                                Info = rpy.Info,
                                                Reference = rpy.Reference,
                                                AmountRepaid = rpy.Repaid,
                                                Ignore = rpy.Ignore,
                                                Settled = rpy.Settled
                                            })
                             .OrderByDescending(rpy => rpy.BookingDate)
                             .ToListAsync(cancellationToken);
        return payments;
    }
}