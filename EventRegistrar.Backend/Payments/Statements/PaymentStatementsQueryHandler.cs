using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Payments.Unassigned;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Payments.Statements
{
    public class PaymentStatementsQueryHandler : IRequestHandler<PaymentStatementsQuery, IEnumerable<PaymentDisplayItem>>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly IQueryable<ReceivedPayment> _payments;

        public PaymentStatementsQueryHandler(IQueryable<ReceivedPayment> payments,
            IEventAcronymResolver acronymResolver)
        {
            _payments = payments;
            _acronymResolver = acronymResolver;
        }

        public async Task<IEnumerable<PaymentDisplayItem>> Handle(PaymentStatementsQuery request, CancellationToken cancellationToken)
        {
            var eventId = await _acronymResolver.GetEventIdFromAcronym(request.EventAcronym);
            var payments = await _payments
                                 .Where(rpy => rpy.PaymentFile.EventId == eventId
                                            && !rpy.Settled)
                                 .Select(rpy => new PaymentDisplayItem
                                 {
                                     Id = rpy.Id,
                                     Amount = rpy.Amount,
                                     AmountAssigned = rpy.Assignments.Sum(ass => ass.Amount),
                                     BookingDate = rpy.BookingDate,
                                     Currency = rpy.Currency,
                                     Info = rpy.Info,
                                     Reference = rpy.Reference,
                                     Repaid = rpy.Repaid,
                                     Settled = rpy.Settled
                                 })
                                 .ToListAsync(cancellationToken);
            return payments;
        }
    }
}