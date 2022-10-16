using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Registrations.Cancel;

using MediatR;

namespace EventRegistrar.Backend.Payments.Refunds;

public class RefundsQuery : IRequest<IEnumerable<RefundDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class RefundsQueryHandler : IRequestHandler<RefundsQuery, IEnumerable<RefundDisplayItem>>
{
    private readonly IQueryable<RegistrationCancellation> _cancellations;

    public RefundsQueryHandler(IQueryable<RegistrationCancellation> cancellations)
    {
        _cancellations = cancellations;
    }

    public async Task<IEnumerable<RefundDisplayItem>> Handle(RefundsQuery query, CancellationToken cancellationToken)
    {
        var refunds = await _cancellations
                            .Where(cnc => cnc.Registration!.EventId == query.EventId
                                       && cnc.Refund > 0m)
                            .Select(cnc => new RefundDisplayItem
                                           {
                                               RegistrationId = cnc.RegistrationId,
                                               FirstName = cnc.Registration.RespondentFirstName,
                                               LastName = cnc.Registration.RespondentLastName,
                                               Price = (cnc.Registration.Price ?? 0m) - cnc.Registration.IndividualReductions.Sum(red => red.Amount),
                                               Paid = cnc.Registration.PaymentAssignments.Sum(asn => asn.PayoutRequestId == null
                                                                                                         ? asn.Amount
                                                                                                         : -asn.Amount),
                                               RefundPercentage = cnc.RefundPercentage,
                                               Refund = cnc.Refund,
                                               CancellationDate = cnc.Received ?? cnc.Created,
                                               CancellationReason = cnc.Reason
                                           })
                            .OrderByDescending(rpy => rpy.CancellationDate)
                            .ToListAsync(cancellationToken);
        return refunds;
    }
}

public class RefundDisplayItem
{
    public Guid RegistrationId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public decimal? Price { get; set; }
    public decimal Paid { get; set; }
    public decimal RefundPercentage { get; set; }
    public decimal Refund { get; set; }
    public DateTimeOffset CancellationDate { get; set; }
    public string CancellationReason { get; set; }
}