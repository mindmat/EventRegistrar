using EventRegistrar.Backend.Payments.Files;

namespace EventRegistrar.Backend.Payments.Assignments;

public class PossibleRepaymentAssignmentQuery : IRequest<IEnumerable<PossibleRepaymentAssignment>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid PaymentId { get; set; }
}

public class PossibleRepaymentAssignmentQueryHandler : IRequestHandler<PossibleRepaymentAssignmentQuery, IEnumerable<PossibleRepaymentAssignment>>
{
    private readonly IQueryable<IncomingPayment> _payments;

    public PossibleRepaymentAssignmentQueryHandler(IQueryable<IncomingPayment> payments)
    {
        _payments = payments;
    }

    public async Task<IEnumerable<PossibleRepaymentAssignment>> Handle(PossibleRepaymentAssignmentQuery query,
                                                                       CancellationToken cancellationToken)
    {
        var payment = await _payments.Where(pmt => pmt.Id == query.PaymentId
                                                && pmt.Payment!.PaymentsFile!.EventId == query.EventId)
                                     .Include(pmt => pmt.Assignments)
                                     .FirstAsync(cancellationToken);

        var payments = await _payments.Where(pmt => pmt.Payment!.PaymentsFile!.EventId == query.EventId
                                                 && !pmt.Payment!.Settled)
                                      .Select(pmt => new PossibleRepaymentAssignment
                                                     {
                                                         PaymentId_OpenPosition = payment.Id,
                                                         PaymentId_Counter = pmt.Id,
                                                         BookingDate = pmt.Payment!.BookingDate,
                                                         Amount = pmt.Payment!.Amount,
                                                         AmountUnsettled = pmt.Payment!.Amount
                                                                         - pmt.Assignments!
                                                                              .Select(asn => asn.PayoutRequestId == null
                                                                                                 ? asn.Amount
                                                                                                 : -asn.Amount)
                                                                              .Sum(),
                                                         Settled = pmt.Payment!.Settled,
                                                         Currency = pmt.Payment!.Currency,
                                                         Info = pmt.Payment!.Info,
                                                         DebitorName = pmt.DebitorName
                                                     })
                                      .ToListAsync(cancellationToken);
        payments.ForEach(pmt => pmt.MatchScore = CalculateMatchScore(pmt, payment));
        return payments.Where(mat => mat.MatchScore > 1)
                       .OrderByDescending(mtc => mtc.MatchScore);
    }

    private static int CalculateMatchScore(PossibleRepaymentAssignment paymentCandidate, IncomingPayment openPayment)
    {
        var debitorParts = paymentCandidate.DebitorName?.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries)
                                           ?.Select(wrd => wrd.ToLowerInvariant())
                        ?? new List<string>();

        var wordsInCandidate = paymentCandidate.Info?.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries)
                                               ?.Select(wrd => wrd.ToLowerInvariant())
                                               ?.ToList()
                            ?? new List<string>();

        var unsettledAmountInOpenPayment = openPayment.Payment!.Amount
                                         - openPayment.Assignments!.Sum(asn => asn.PayoutRequestId == null
                                                                                   ? asn.Amount
                                                                                   : -asn.Amount);
        var wordsInOpenPayment = openPayment.Payment!.Info!
                                            .Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(wrd => wrd.ToLowerInvariant())
                                            .ToHashSet();

        return wordsInOpenPayment.Sum(opw => wordsInCandidate.Count(cdw => cdw == opw))
             + (wordsInOpenPayment.Sum(opw => debitorParts.Count(cdw => cdw == opw)) * 10)
             + (paymentCandidate.AmountUnsettled == unsettledAmountInOpenPayment ? 5 : 0);
    }
}

public class PossibleRepaymentAssignment
{
    public decimal Amount { get; set; }
    public decimal AmountUnsettled { get; set; }
    public DateTime BookingDate { get; set; }
    public string? Currency { get; set; }
    public string? DebitorName { get; set; }
    public string? Info { get; set; }
    public int MatchScore { get; set; }
    public Guid PaymentId_Counter { get; set; }
    public Guid PaymentId_OpenPosition { get; set; }
    public bool Settled { get; set; }
}