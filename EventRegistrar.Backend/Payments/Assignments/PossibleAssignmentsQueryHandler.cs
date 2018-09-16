using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Registrations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Payments.Assignments
{
    public class PossibleAssignmentsQueryHandler : IRequestHandler<PossibleAssignmentsQuery, IEnumerable<PossibleAssignment>>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly IQueryable<ReceivedPayment> _payments;
        private readonly IQueryable<Registration> _registrations;

        public PossibleAssignmentsQueryHandler(IQueryable<ReceivedPayment> payments,
            IQueryable<Registration> registrations,
            IEventAcronymResolver acronymResolver)
        {
            _payments = payments;
            _registrations = registrations;
            _acronymResolver = acronymResolver;
        }

        public async Task<IEnumerable<PossibleAssignment>> Handle(PossibleAssignmentsQuery query, CancellationToken cancellationToken)
        {
            var eventId = await _acronymResolver.GetEventIdFromAcronym(query.EventAcronym);
            var payment = await _payments.FirstAsync(pmt => pmt.Id == query.PaymentId
                                                            && pmt.PaymentFile.EventId == eventId, cancellationToken);
            var info = payment.Info;
            //var infoPrefix = "MITTEILUNGEN:";
            //var infoIndex = payment.Info.IndexOf(infoPrefix, StringComparison.InvariantCultureIgnoreCase);
            //if (infoIndex >= 0)
            //{
            //    info = info.Substring(infoIndex + infoPrefix.Length);
            //}

            var words = info.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var registrations = await _registrations.Where(reg => reg.EventId == eventId
                                                                  && (words.Contains(reg.RespondentFirstName)
                                                                      || words.Contains(reg.RespondentLastName))
                                                                  && reg.State == RegistrationState.Received)
                                                    .Select(reg => new PossibleAssignment
                                                    {
                                                        PaymentId = payment.Id,
                                                        RegistrationId = reg.Id,
                                                        FirstName = reg.RespondentFirstName,
                                                        LastName = reg.RespondentLastName,
                                                        Amount = reg.Price ?? 0m,
                                                        AmountPaid = reg.Payments.Sum(pmt => pmt.Amount)
                                                    })
                                                    .ToListAsync(cancellationToken);
            return registrations;
        }
    }
}