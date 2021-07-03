using System.Collections.Generic;

using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Assignments;

using MediatR;

namespace EventRegistrar.Backend.Payments.Refunds
{
    public class CheckIfPayoutIsConfirmedWhenPaymentAssigned : IEventToCommandTranslation<PaymentAssigned>
    {
        public IEnumerable<IRequest> Translate(PaymentAssigned e)
        {
            if (e.PayoutRequestId != null)
            {
                yield return new CheckIfPayoutIsConfirmedCommand { PayoutRequestId = e.PayoutRequestId.Value };
            }
        }
    }
}