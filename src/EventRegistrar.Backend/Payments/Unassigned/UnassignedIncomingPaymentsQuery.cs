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
        var payments = await _payments.Where(rpy => rpy.Payment!.PaymentsFile!.EventId == query.EventId
                                                 && !rpy.Payment.Settled
                                                 && !rpy.Payment.Ignore)
                                      .Select(rpy => new PaymentDisplayItem
                                                     {
                                                         Id = rpy.Id,
                                                         Amount = rpy.Payment!.Amount,
                                                         AmountAssigned = rpy.Assignments!.Sum(asn => asn.PayoutRequestId == null
                                                                                                          ? asn.Amount
                                                                                                          : -asn.Amount),
                                                         BookingDate = rpy.Payment.BookingDate,
                                                         Currency = rpy.Payment.Currency,
                                                         Info = rpy.Payment.Info,
                                                         Reference = rpy.Payment.Reference,
                                                         AmountRepaid = rpy.Payment.Repaid,
                                                         Settled = rpy.Payment.Settled,
                                                         PaymentSlipId = rpy.PaymentSlipId,
                                                         Message = rpy.Payment.Message,
                                                         DebitorName = rpy.DebitorName
                                                     })
                                      .OrderByDescending(rpy => rpy.BookingDate)
                                      .ThenByDescending(rpy => rpy.Amount)
                                      .ToListAsync(cancellationToken);
        return payments;
    }
}