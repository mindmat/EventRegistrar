using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Payments.Unassigned;

using MediatR;

namespace EventRegistrar.Backend.Payments.Statements;

public class BankAccountBookingsQuery : IRequest<IEnumerable<PaymentDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class BankAccountBookingsQueryHandler : IRequestHandler<BankAccountBookingsQuery, IEnumerable<PaymentDisplayItem>>
{
    private readonly IQueryable<BankAccountBooking> _payments;

    public BankAccountBookingsQueryHandler(IQueryable<BankAccountBooking> payments)
    {
        _payments = payments;
    }

    public async Task<IEnumerable<PaymentDisplayItem>> Handle(BankAccountBookingsQuery query,
                                                              CancellationToken cancellationToken)
    {
        var payments = await _payments
                             .Where(rpy => rpy.BankAccountStatementsFile!.EventId == query.EventId)
                             .Select(rpy => new PaymentDisplayItem
                                            {
                                                Id = rpy.Id,
                                                Amount = rpy.Amount,
                                                AmountAssigned = rpy.Assignments!.Sum(asn =>
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