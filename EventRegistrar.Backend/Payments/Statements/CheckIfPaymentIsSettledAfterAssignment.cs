using EventRegistrar.Backend.Infrastructure.Events;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Payments.Assignments;

namespace EventRegistrar.Backend.Payments.Statements
{
    public class CheckIfPaymentIsSettledAfterAssignment : IEventToCommandTranslation<PaymentAssignedEvent>
    {
        public IQueueBoundMessage Translate(PaymentAssignedEvent @event)
        {
            return new CheckIfPaymentIsSettledCommand { PaymentId = @event.PaymentId };
        }
    }
}