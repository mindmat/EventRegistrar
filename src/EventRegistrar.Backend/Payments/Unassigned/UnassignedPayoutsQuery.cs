using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Payments.Files.Camt;

using MediatR;

namespace EventRegistrar.Backend.Payments.Unassigned;

public class UnassignedPayoutsQuery : IRequest<IEnumerable<PaymentDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid PaymentId { get; set; }
}

public class UnassignedPayoutsQueryHandler : IRequestHandler<UnassignedPayoutsQuery, IEnumerable<PaymentDisplayItem>>
{
    private readonly IQueryable<BankAccountBooking> _payments;

    public UnassignedPayoutsQueryHandler(IQueryable<BankAccountBooking> payments)
    {
        _payments = payments;
    }

    public async Task<IEnumerable<PaymentDisplayItem>> Handle(UnassignedPayoutsQuery query,
                                                              CancellationToken cancellationToken)
    {
        var payments = await _payments
                             .Where(rpy => rpy.BankAccountStatementsFile.EventId == query.EventId
                                        && !rpy.Settled
                                        && !rpy.Ignore
                                        && rpy.CreditDebitType == CreditDebit.DBIT)
                             .Select(pmo => new PaymentDisplayItem
                                            {
                                                Id = pmo.Id,
                                                Amount = pmo.Amount,
                                                AmountAssigned = pmo.Assignments.Sum(asn =>
                                                    asn.PayoutRequestId == null ? asn.Amount : -asn.Amount),
                                                BookingDate = pmo.BookingDate,
                                                Currency = pmo.Currency,
                                                Info = pmo.Info,
                                                Reference = pmo.Reference,
                                                AmountRepaid = pmo.Repaid,
                                                Settled = pmo.Settled,
                                                Message = pmo.Message,
                                                CreditorName = pmo.CreditorName,
                                                CreditorIban = pmo.CreditorIban
                                            })
                             .OrderByDescending(rpy => rpy.BookingDate)
                             .ThenByDescending(rpy => rpy.Amount)
                             .ToListAsync(cancellationToken);
        return payments;
    }
}