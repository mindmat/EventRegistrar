using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Payments.Files;

using MediatR;

namespace EventRegistrar.Backend.Payments.Unassigned;

public class UnassignedIncomingPaymentsQuery : IRequest<IEnumerable<PaymentDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid PaymentId { get; set; }
}

public class UnassignedIncomingPaymentsQueryHandler : IRequestHandler<UnassignedIncomingPaymentsQuery, IEnumerable<PaymentDisplayItem>>
{
    private readonly IQueryable<IncomingPayment> _payments;

    public UnassignedIncomingPaymentsQueryHandler(IQueryable<IncomingPayment> payments)
    {
        _payments = payments;
    }

    public async Task<IEnumerable<PaymentDisplayItem>> Handle(UnassignedIncomingPaymentsQuery query,
                                                              CancellationToken cancellationToken)
    {
        var payments = await _payments.Where(rpy => rpy.BankAccountStatementsFile!.EventId == query.EventId
                                                 && !rpy.Settled
                                                 && !rpy.Ignore)
                                      .Select(rpy => new PaymentDisplayItem
                                                     {
                                                         Id = rpy.Id,
                                                         Amount = rpy.Amount,
                                                         AmountAssigned = rpy.Assignments!.Sum(asn => asn.PayoutRequestId == null
                                                                                                          ? asn.Amount
                                                                                                          : -asn.Amount),
                                                         BookingDate = rpy.BookingDate,
                                                         Currency = rpy.Currency,
                                                         Info = rpy.Info,
                                                         Reference = rpy.Reference,
                                                         AmountRepaid = rpy.Repaid,
                                                         Settled = rpy.Settled,
                                                         PaymentSlipId = rpy.PaymentSlipId,
                                                         Message = rpy.Message,
                                                         DebitorName = rpy.DebitorName
                                                     })
                                      .OrderByDescending(rpy => rpy.BookingDate)
                                      .ThenByDescending(rpy => rpy.Amount)
                                      .ToListAsync(cancellationToken);
        return payments;
    }
}