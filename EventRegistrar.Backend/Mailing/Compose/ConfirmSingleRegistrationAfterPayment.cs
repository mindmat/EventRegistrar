using EventRegistrar.Backend.Infrastructure.Events;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Registrations.Confirmation;

namespace EventRegistrar.Backend.Mailing.Compose
{
    public class ConfirmSingleRegistrationAfterPayment : IEventToCommandTranslation<SingleRegistrationPaid>
    {
        public IQueueBoundMessage Translate(SingleRegistrationPaid @event)
        {
            return new ComposeAndSendMailCommand { MailType = MailType.SingleRegistrationFullyPaid, RegistrationId = @event.RegistrationId, Withhold = true };
        }
    }
}