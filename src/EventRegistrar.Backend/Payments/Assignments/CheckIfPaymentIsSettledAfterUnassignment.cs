using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Payments.Statements;
using MediatR;

namespace EventRegistrar.Backend.Payments.Assignments;

public class CheckIfPaymentIsSettledAfterUnassignment : IEventToCommandTranslation<PaymentUnassigned>
{
    public IEnumerable<IRequest> Translate(PaymentUnassigned e)
    {
        yield return new CheckIfPaymentIsSettledCommand { PaymentId = e.PaymentId };
    }
}