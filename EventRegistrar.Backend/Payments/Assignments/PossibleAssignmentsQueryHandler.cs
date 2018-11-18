﻿using System;
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
            var payment = await _payments.FirstAsync(pmt => pmt.Id == query.PaymentId
                                                            && pmt.PaymentFile.EventId == query.EventId, cancellationToken);
            var info = payment.Info;

            var wordsInPayment = info.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var registrations = await _registrations.Where(reg => reg.EventId == query.EventId
                                                                  && (wordsInPayment.Contains(reg.RespondentFirstName)
                                                                   || wordsInPayment.Contains(reg.RespondentLastName)
                                                                   || wordsInPayment.Contains(reg.RespondentEmail))
                                                                  && reg.State == RegistrationState.Received
                                                                  && reg.IsWaitingList == false)
                                                    .Select(reg => new PossibleAssignment
                                                    {
                                                        PaymentId = payment.Id,
                                                        RegistrationId = reg.Id,
                                                        FirstName = reg.RespondentFirstName,
                                                        LastName = reg.RespondentLastName,
                                                        Amount = reg.Price ?? 0m,
                                                        AmountPaid = reg.Payments.Sum(pmt => pmt.Amount),
                                                        MatchScore = (wordsInPayment.Count(wrd => wrd == reg.RespondentFirstName)) +
                                                                     (wordsInPayment.Count(wrd => wrd == reg.RespondentLastName)) +
                                                                     (wordsInPayment.Count(wrd => wrd == reg.RespondentEmail) * 5),
                                                        IsWaitingList = reg.IsWaitingList == true
                                                    })
                                                    .OrderByDescending(mtc => mtc.MatchScore)
                                                    .ToListAsync(cancellationToken);
            return registrations;
        }
    }
}