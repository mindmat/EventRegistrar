using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Assignments;

namespace EventRegistrar.Backend.Payments.Statements;

public class CheckIfPaymentIsSettledAfterAssignment : IEventToCommandTranslation<IncomingPaymentAssigned>
{
    public IEnumerable<IRequest> Translate(IncomingPaymentAssigned e)
    {
        yield return new CheckIfIncomingPaymentIsSettledCommand { IncomingPaymentId = e.IncomingPaymentId };
        //if (e.PaymentId_Counter != null)
        //{
        //    yield return new CheckIfIncomingPaymentIsSettledCommand { IncomingPaymentId = e.PaymentId_Counter.Value };
        //}
    }
}