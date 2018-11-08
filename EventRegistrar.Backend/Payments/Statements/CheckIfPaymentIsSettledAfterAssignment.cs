using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Payments.Assignments;

namespace EventRegistrar.Backend.Payments.Statements
{
    public class CheckIfPaymentIsSettledAfterAssignment : IEventToCommandTranslation<PaymentAssignedEvent>
    {
        public IQueueBoundMessage Translate(PaymentAssignedEvent e)
        {
            return new CheckIfPaymentIsSettledCommand { PaymentId = e.PaymentId };
        }
    }
}