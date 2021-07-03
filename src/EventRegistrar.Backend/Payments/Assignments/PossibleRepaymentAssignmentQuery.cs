using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Payments.Files.Camt;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Payments.Assignments
{
    public class PossibleRepaymentAssignmentQuery : IRequest<IEnumerable<PossibleRepaymentAssignment>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public Guid PaymentId { get; set; }
    }

    public class PossibleRepaymentAssignmentQueryHandler : IRequestHandler<PossibleRepaymentAssignmentQuery, IEnumerable<PossibleRepaymentAssignment>>
    {
        private readonly IQueryable<ReceivedPayment> _payments;

        public PossibleRepaymentAssignmentQueryHandler(IQueryable<ReceivedPayment> payments)
        {
            _payments = payments;
        }

        public async Task<IEnumerable<PossibleRepaymentAssignment>> Handle(PossibleRepaymentAssignmentQuery query, CancellationToken cancellationToken)
        {
            var payment = await _payments.Where(pmt => pmt.Id == query.PaymentId
                                                    && pmt.PaymentFile.EventId == query.EventId)
                                         .Include(pmt => pmt.Assignments)
                                         .FirstAsync(cancellationToken);

            var payments = await _payments.Where(pmt => pmt.PaymentFile.EventId == query.EventId
                                                        && !pmt.Settled
                                                        && pmt.CreditDebitType == CreditDebit.DBIT)
                                          .Select(pmt => new PossibleRepaymentAssignment
                                          {
                                              PaymentId_OpenPosition = payment.Id,
                                              PaymentId_Counter = pmt.Id,
                                              BookingDate = pmt.BookingDate,
                                              Amount = pmt.Amount,
                                              AmountUnsettled = pmt.Amount - pmt.Assignments.Select(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount).Sum(),
                                              Settled = pmt.Settled,
                                              Currency = pmt.Currency,
                                              Info = pmt.Info,
                                              DebitorName = pmt.DebitorName
                                          })
                                          .ToListAsync(cancellationToken);
            payments.ForEach(pmt => pmt.MatchScore = CalculateMatchScore(pmt, payment));
            return payments
                   .Where(mat => mat.MatchScore > 1)
                   .OrderByDescending(mtc => mtc.MatchScore);
        }

        private static int CalculateMatchScore(PossibleRepaymentAssignment paymentCandidate, ReceivedPayment openPayment)
        {
            var debitorParts = paymentCandidate.DebitorName?.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries)?.Select(wrd => wrd.ToLowerInvariant()) ?? new List<string>();
            var wordsInCandidate = paymentCandidate.Info?.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries)?.Select(wrd => wrd.ToLowerInvariant())?.ToList() ?? new List<string>();

            var unsettledAmountInOpenPayment = openPayment.Amount - openPayment.Assignments.Sum(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount);
            var wordsInOpenPayment = openPayment.Info.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries).Select(wrd => wrd.ToLowerInvariant()).ToHashSet();

            return wordsInOpenPayment.Sum(opw => wordsInCandidate.Count(cdw => cdw == opw))
                   + wordsInOpenPayment.Sum(opw => debitorParts.Count(cdw => cdw == opw)) * 10
                   + (paymentCandidate.AmountUnsettled == unsettledAmountInOpenPayment ? 5 : 0);
        }
    }

    public class PossibleRepaymentAssignment
    {
        public decimal Amount { get; set; }
        public decimal AmountUnsettled { get; set; }
        public DateTime BookingDate { get; set; }
        public string Currency { get; set; }
        public string DebitorName { get; set; }
        public string Info { get; set; }
        public int MatchScore { get; set; }
        public Guid PaymentId_Counter { get; set; }
        public Guid PaymentId_OpenPosition { get; set; }
        public bool Settled { get; set; }
    }

}