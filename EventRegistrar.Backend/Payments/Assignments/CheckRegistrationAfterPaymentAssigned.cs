using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Registrations.Confirmation;

namespace EventRegistrar.Backend.Payments.Assignments
{
    public class CheckRegistrationAfterPaymentAssigned : IEventToCommandTranslation<PaymentAssigned>
    {
        public IQueueBoundMessage Translate(PaymentAssigned e)
        {
            return new CheckRegistrationAfterPaymentCommand { RegistrationId = e.RegistrationId };
        }
    }
}