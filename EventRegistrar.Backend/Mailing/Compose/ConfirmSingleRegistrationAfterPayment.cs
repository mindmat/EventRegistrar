using EventRegistrar.Backend.Infrastructure.Events;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Registrations.Confirmation;

namespace EventRegistrar.Backend.Mailing.Compose
{
    public class ConfirmSingleRegistrationAfterPayment : IEventToCommandTranslation<SingleRegistrationPaid>
    {
        public IQueueBoundMessage Translate(SingleRegistrationPaid e)
        {
            return new ComposeAndSendMailCommand { MailType = MailType.SingleRegistrationFullyPaid, RegistrationId = e.RegistrationId, Withhold = true };
        }
    }
}