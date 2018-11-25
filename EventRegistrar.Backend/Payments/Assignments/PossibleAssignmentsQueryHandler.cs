using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Registrations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Payments.Assignments
{
    public class PossibleAssignmentsQueryHandler : IRequestHandler<PossibleAssignmentsQuery, IEnumerable<PossibleAssignment>>
    {
        private readonly IQueryable<ReceivedPayment> _payments;
        private readonly IQueryable<Registration> _registrations;

        public PossibleAssignmentsQueryHandler(IQueryable<ReceivedPayment> payments,
                                               IQueryable<Registration> registrations)
        {
            _payments = payments;
            _registrations = registrations;
        }

        public async Task<IEnumerable<PossibleAssignment>> Handle(PossibleAssignmentsQuery query, CancellationToken cancellationToken)
        {
            var payment = await _payments.Where(pmt => pmt.Id == query.PaymentId
                                                    && pmt.PaymentFile.EventId == query.EventId)
                                         .Include(pmt => pmt.Assignments)
                                         .FirstAsync(cancellationToken);
            var info = payment.Info;
            var openAmount = payment.Amount - payment.Assignments.Sum(ass => ass.Amount);

            var wordsInPayment = info.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(wrd => wrd.ToLowerInvariant()).ToHashSet();
            var registrations = await _registrations.Where(reg => reg.EventId == query.EventId
                                                                  && (info.Contains(reg.RespondentFirstName)
                                                                   || info.Contains(reg.RespondentLastName)
                                                                   || info.Contains(reg.RespondentEmail))
                                                                  && reg.State == RegistrationState.Received)

                                                    .Select(reg => new PossibleAssignment
                                                    {
                                                        PaymentId = payment.Id,
                                                        RegistrationId = reg.Id,
                                                        FirstName = reg.RespondentFirstName,
                                                        LastName = reg.RespondentLastName,
                                                        Email = reg.RespondentEmail,
                                                        Amount = reg.Price ?? 0m,
                                                        AmountPaid = reg.Payments.Sum(pmt => pmt.Amount),
                                                        IsWaitingList = reg.IsWaitingList == true,
                                                    })
                                                    .ToListAsync(cancellationToken);
            registrations.ForEach(reg => reg.MatchScore = CalculateMatchScore(reg, wordsInPayment, openAmount));
            registrations.ForEach(reg => reg.AmountMatch = openAmount == reg.Amount - reg.AmountPaid);
            return registrations
                   .Where(mat => mat.MatchScore > 0)
                   .OrderByDescending(mtc => mtc.MatchScore);
        }

        private static int CalculateMatchScore(PossibleAssignment reg, HashSet<string> wordsInPayment, decimal openAmount)
        {
            // names can contain multiple words, e.g. 'de Luca'
            var nameWords = reg.FirstName.Split(' ').Union(reg.LastName.Split(' ')).Select(nmw => nmw.ToLowerInvariant()).ToList();
            var nameScore = nameWords.Sum(nmw => wordsInPayment.Count(wrd => wrd == nmw));
            var mailaddressScore = wordsInPayment.Contains(reg.Email) ? 5 : 0;
            return nameScore + mailaddressScore;
        }
    }
}