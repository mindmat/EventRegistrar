using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Payments.Unassigned
{
    public class UnassignedPaymentsQueryHandler : IRequestHandler<UnassignedPaymentsQuery, IEnumerable<PaymentDisplayItem>>
    {
        private readonly IQueryable<ReceivedPayment> _payments;

        public UnassignedPaymentsQueryHandler(IQueryable<ReceivedPayment> payments)
        {
            _payments = payments;
        }

        public async Task<IEnumerable<PaymentDisplayItem>> Handle(UnassignedPaymentsQuery query, CancellationToken cancellationToken)
        {
            var payments = await _payments
                                 .Where(rpy => rpy.PaymentFile.EventId == query.EventId &&
                                               !rpy.Settled)
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
                                     Settled = rpy.Settled,
                                     PaymentSlipId = rpy.PaymentSlipId
                                 })
                                 .ToListAsync(cancellationToken);
            return payments;
        }
    }
}