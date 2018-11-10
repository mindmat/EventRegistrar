using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Payments.Unassigned;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Payments.Statements
{
    public class PaymentStatementsQueryHandler : IRequestHandler<PaymentStatementsQuery, IEnumerable<PaymentDisplayItem>>
    {
        private readonly IQueryable<ReceivedPayment> _payments;

        public PaymentStatementsQueryHandler(IQueryable<ReceivedPayment> payments)
        {
            _payments = payments;
        }

        public async Task<IEnumerable<PaymentDisplayItem>> Handle(PaymentStatementsQuery query, CancellationToken cancellationToken)
        {
            var payments = await _payments
                                 .Where(rpy => rpy.PaymentFile.EventId == query.EventId)
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
                                 .OrderByDescending(rpy => rpy.BookingDate)
                                 .ToListAsync(cancellationToken);
            return payments;
        }
    }
}