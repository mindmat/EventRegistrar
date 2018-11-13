using System.Collections.Generic;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Payments.Assignments;

namespace EventRegistrar.Backend.Payments.Statements
{
    public class CheckIfPaymentIsSettledAfterAssignment : IEventToCommandTranslation<PaymentAssigned>
    {
        public IEnumerable<IQueueBoundMessage> Translate(PaymentAssigned e)
        {
            yield return new CheckIfPaymentIsSettledCommand { PaymentId = e.PaymentId };
        }
    }
}