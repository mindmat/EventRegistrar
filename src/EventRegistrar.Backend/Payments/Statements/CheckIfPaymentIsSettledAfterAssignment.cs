using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Assignments;

using MediatR;

namespace EventRegistrar.Backend.Payments.Statements;

public class CheckIfPaymentIsSettledAfterAssignment : IEventToCommandTranslation<PaymentAssigned>
{
    public IEnumerable<IRequest> Translate(PaymentAssigned e)
    {
        yield return new CheckIfPaymentIsSettledCommand { PaymentId = e.PaymentId };
        if (e.PaymentId_Counter != null)
        {
            yield return new CheckIfPaymentIsSettledCommand { PaymentId = e.PaymentId_Counter.Value };
        }
    }
}