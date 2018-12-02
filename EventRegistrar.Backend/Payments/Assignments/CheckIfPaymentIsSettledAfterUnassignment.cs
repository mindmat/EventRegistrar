using System.Collections.Generic;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Payments.Statements;

namespace EventRegistrar.Backend.Payments.Assignments
{
    public class CheckIfPaymentIsSettledAfterUnassignment : IEventToCommandTranslation<PaymentUnassigned>
    {
        public IEnumerable<IQueueBoundMessage> Translate(PaymentUnassigned e)
        {
            yield return new CheckIfPaymentIsSettledCommand { PaymentId = e.PaymentId };
        }
    }
}