using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Statements;

namespace EventRegistrar.Backend.Payments.Assignments;

public class CheckIfPaymentIsSettledAfterUnassignment : IEventToCommandTranslation<IncomingPaymentUnassigned>
{
    public IEnumerable<IRequest> Translate(IncomingPaymentUnassigned e)
    {
        yield return new CheckIfIncomingPaymentIsSettledCommand { IncomingPaymentId = e.IncomingPaymentId };
    }
}