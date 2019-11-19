using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Payments.Files.Camt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Payments.Assignments
{
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
                                              AmountUnsettled = pmt.Amount - pmt.Assignments.Select(ass => ass.Amount).Sum(),
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

            var unsettledAmountInOpenPayment = openPayment.Amount - openPayment.Assignments.Sum(ass => ass.Amount);
            var wordsInOpenPayment = openPayment.Info.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries).Select(wrd => wrd.ToLowerInvariant()).ToHashSet();

            return wordsInOpenPayment.Sum(opw => wordsInCandidate.Count(cdw => cdw == opw))
                   + wordsInOpenPayment.Sum(opw => debitorParts.Count(cdw => cdw == opw)) * 10
                   + (paymentCandidate.AmountUnsettled == unsettledAmountInOpenPayment ? 5 : 0);
        }
    }
}