using EventRegistrar.Backend.Payments.Files;

namespace EventRegistrar.Backend.Payments.Unassigned;

public class UnassignedPayoutsQuery : IRequest<IEnumerable<PaymentDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid PaymentId { get; set; }
}

public class UnassignedPayoutsQueryHandler : IRequestHandler<UnassignedPayoutsQuery, IEnumerable<PaymentDisplayItem>>
{
    private readonly IQueryable<OutgoingPayment> _payments;

    public UnassignedPayoutsQueryHandler(IQueryable<OutgoingPayment> payments)
    {
        _payments = payments;
    }

    public async Task<IEnumerable<PaymentDisplayItem>> Handle(UnassignedPayoutsQuery query,
                                                              CancellationToken cancellationToken)
    {
        var payments = await _payments.Where(rpy => rpy.Payment!.EventId == query.EventId
                                                 && !rpy.Payment!.Settled
                                                 && !rpy.Payment!.Ignore)
                                      .Select(pmo => new PaymentDisplayItem
                                                     {
                                                         Id = pmo.Id,
                                                         Amount = pmo.Payment!.Amount,
                                                         AmountAssigned = pmo.Assignments!.Sum(asn => asn.PayoutRequestId == null
                                                                                                          ? asn.Amount
                                                                                                          : -asn.Amount),
                                                         BookingDate = pmo.Payment.BookingDate,
                                                         Currency = pmo.Payment.Currency,
                                                         Info = pmo.Payment.Info,
                                                         Reference = pmo.Payment.Reference,
                                                         AmountRepaid = pmo.Payment.Repaid,
                                                         Settled = pmo.Payment.Settled,
                                                         Message = pmo.Payment.Message,
                                                         CreditorName = pmo.CreditorName,
                                                         CreditorIban = pmo.CreditorIban
                                                     })
                                      .OrderByDescending(rpy => rpy.BookingDate)
                                      .ThenByDescending(rpy => rpy.Amount)
                                      .ToListAsync(cancellationToken);
        return payments;
    }
}