using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Assignments;

using MediatR;

namespace EventRegistrar.Backend.Payments.Refunds;

public class CheckIfPayoutIsConfirmedWhenOutgoingPaymentAssigned : IEventToCommandTranslation<OutgoingPaymentAssigned>
{
    public IEnumerable<IRequest> Translate(OutgoingPaymentAssigned e)
    {
        if (e.PayoutRequestId != null)
        {
            yield return new CheckIfPayoutIsConfirmedCommand { PayoutRequestId = e.PayoutRequestId.Value };
        }
    }
}