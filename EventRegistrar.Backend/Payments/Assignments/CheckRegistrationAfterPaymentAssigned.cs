using EventRegistrar.Backend.Infrastructure.Events;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Registrations.Confirmation;

namespace EventRegistrar.Backend.Payments.Assignments
{
    public class CheckRegistrationAfterPaymentAssigned : IEventToCommandTranslation<PaymentAssignedEvent>
    {
        public IQueueBoundMessage Translate(PaymentAssignedEvent @event)
        {
            return new CheckRegistrationAfterPaymentCommand { RegistrationId = @event.RegistrationId };
        }
    }
}