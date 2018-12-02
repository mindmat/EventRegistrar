using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Payments.Assignments
{
    public class AssignedPaymentsOfRegistrationQueryHandler : IRequestHandler<AssignedPaymentsOfRegistrationQuery, IEnumerable<AssignedPaymentDisplayItem>>
    {
        private readonly IQueryable<PaymentAssignment> _paymentsAssignments;

        public AssignedPaymentsOfRegistrationQueryHandler(IQueryable<PaymentAssignment> paymentsAssignments)
        {
            _paymentsAssignments = paymentsAssignments;
        }

        public async Task<IEnumerable<AssignedPaymentDisplayItem>> Handle(AssignedPaymentsOfRegistrationQuery query, CancellationToken cancellationToken)
        {
            return await _paymentsAssignments.Where(pya => pya.ReceivedPayment.PaymentFile.EventId == query.EventId
                                                        && pya.RegistrationId == query.RegistrationId)
                                             .Select(pya => new AssignedPaymentDisplayItem
                                             {
                                                 PaymentAssignmentId = pya.Id,
                                                 Amount = pya.Amount,
                                                 Currency = pya.ReceivedPayment.Currency,
                                                 BookingDate = pya.ReceivedPayment.BookingDate
                                             })
                                             .ToListAsync(cancellationToken);
        }
    }
}