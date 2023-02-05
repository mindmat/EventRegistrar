using EventRegistrar.Backend.Registrations.Cancel;

namespace EventRegistrar.Backend.Payments.Refunds;

public class CancellationsQuery : IRequest<IEnumerable<CancellationDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class CancellationsQueryHandler : IRequestHandler<CancellationsQuery, IEnumerable<CancellationDisplayItem>>
{
    private readonly IQueryable<RegistrationCancellation> _cancellations;

    public CancellationsQueryHandler(IQueryable<RegistrationCancellation> cancellations)
    {
        _cancellations = cancellations;
    }

    public async Task<IEnumerable<CancellationDisplayItem>> Handle(CancellationsQuery query, CancellationToken cancellationToken)
    {
        var cancellations = await _cancellations.Where(cnc => cnc.Registration!.EventId == query.EventId)
                                                .Include(cnc => cnc.Registration!.PaymentAssignments)
                                                .ToListAsync(cancellationToken);
        return cancellations.Select(cnc => new CancellationDisplayItem
                                           {
                                               RegistrationId = cnc.RegistrationId,
                                               FirstName = cnc.Registration!.RespondentFirstName,
                                               LastName = cnc.Registration!.RespondentLastName,
                                               ReceivedAt = cnc.Registration!.ReceivedAt,
                                               Paid = cnc.Registration.PaymentAssignments!
                                                         .Where(asn => asn.IncomingPaymentId != null)
                                                         .Sum(asn => asn.Amount),
                                               Repaid = cnc.Registration.PaymentAssignments!
                                                           .Where(asn => asn.OutgoingPaymentId != null)
                                                           .Sum(asn => asn.Amount),
                                               RefundPercentage = cnc.RefundPercentage,
                                               Refund = cnc.Refund,
                                               CancelledPer = cnc.Received,
                                               CancelledAt = cnc.Created,
                                               CancellationReason = cnc.Reason
                                           })
                            .OrderByDescending(cnc => cnc.CancelledPer ?? cnc.CancelledAt);
    }
}

public class CancellationDisplayItem
{
    public Guid RegistrationId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTimeOffset? ReceivedAt { get; set; }
    public decimal Paid { get; set; }
    public decimal Repaid { get; set; }
    public decimal RefundPercentage { get; set; }
    public decimal Refund { get; set; }
    public DateTimeOffset? CancelledPer { get; set; }
    public DateTimeOffset CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
}