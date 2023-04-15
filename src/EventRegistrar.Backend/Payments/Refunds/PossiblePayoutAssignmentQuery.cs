using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Payments.Files;

using MediatR;

namespace EventRegistrar.Backend.Payments.Refunds;

public class PossiblePayoutAssignmentQuery : IRequest<IEnumerable<PossiblePayoutAssignment>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid PaymentId { get; set; }
}

public class PossiblePayoutAssignmentQueryHandler : IRequestHandler<PossiblePayoutAssignmentQuery, IEnumerable<PossiblePayoutAssignment>>
{
    private readonly IQueryable<OutgoingPayment> _payments;
    private readonly IQueryable<PayoutRequest> _payoutRequests;

    public PossiblePayoutAssignmentQueryHandler(IQueryable<OutgoingPayment> payments,
                                                IQueryable<PayoutRequest> payoutRequests)
    {
        _payments = payments;
        _payoutRequests = payoutRequests;
    }

    public async Task<IEnumerable<PossiblePayoutAssignment>> Handle(PossiblePayoutAssignmentQuery query,
                                                                    CancellationToken cancellationToken)
    {
        var payment = await _payments.Where(pmt => pmt.Id == query.PaymentId
                                                && pmt.Payment!.EventId == query.EventId)
                                     .Include(pmt => pmt.Payment)
                                     .Include(por => por.Assignments!)
                                     .ThenInclude(asn => asn.IncomingPayment)
                                     .FirstAsync(cancellationToken);

        var openAmount = payment.Payment!.Amount - payment.Assignments!.Sum(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount);

        var payouts = await _payoutRequests.Where(por => por.Registration!.EventId == query.EventId
                                                      && por.State != PayoutState.Confirmed)
                                           .Select(por => new PossiblePayoutAssignment
                                                          {
                                                              RegistrationId = por.RegistrationId,
                                                              PaymentId_OpenPosition = payment.Id,
                                                              PayoutRequestId = por.Id,
                                                              Created = por.Created,
                                                              Amount = por.Amount,
                                                              AmountAssigned = por.Assignments!
                                                                                  .Select(asn => asn.Amount)
                                                                                  .Sum(),
                                                              IsOpen = por.State == PayoutState.Sent,
                                                              Info = por.Reason,
                                                              Participant = por.Registration!.RespondentFirstName + " " + por.Registration.RespondentLastName,
                                                              Ibans = por.Registration.PaymentAssignments!
                                                                         .Select(pmt => pmt.IncomingPayment!.DebitorIban)
                                                                         .Where(ibn => ibn != null)
                                                                         .Select(ibn => ibn!)
                                                                         .ToArray()
                                                          })
                                           .ToListAsync(cancellationToken);
        payouts.ForEach(por => por.AmountMatch = openAmount == por.Amount - por.AmountAssigned);
        payouts.ForEach(pmt => pmt.MatchScore = CalculateMatchScore(pmt, payment));
        return payouts.Where(mat => mat.MatchScore > 1)
                      .OrderByDescending(mtc => mtc.MatchScore);
    }

    private static int CalculateMatchScore(PossiblePayoutAssignment payoutRequestCandidate, OutgoingPayment openPayment)
    {
        var participantParts =
            payoutRequestCandidate.Participant?.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries)
                                  ?.Select(wrd => wrd.ToLowerInvariant())
         ?? new List<string>();

        var wordsInOpenPayment = openPayment.Payment.Info.Split(' ').Select(nmw => nmw.ToLowerInvariant()).ToList();

        return payoutRequestCandidate.Ibans.Contains(openPayment.CreditorIban)
                   ? 50
                   : 0
                   + (payoutRequestCandidate.AmountMatch ? 20 : 0)
                   + (wordsInOpenPayment.Sum(opw => participantParts.Count(cdw => cdw == opw)) * 5);
    }
}

public class PossiblePayoutAssignment
{
    public decimal Amount { get; set; }
    public decimal AmountAssigned { get; set; }
    public bool AmountMatch { get; set; }
    public DateTimeOffset Created { get; set; }
    public string Currency { get; set; }
    public string Participant { get; set; }
    public string? Info { get; set; }
    public int MatchScore { get; set; }
    public Guid PayoutRequestId { get; set; }
    public Guid PaymentId_OpenPosition { get; set; }
    public bool IsOpen { get; set; }
    public IEnumerable<string> Ibans { get; set; }
    public Guid RegistrationId { get; set; }
}