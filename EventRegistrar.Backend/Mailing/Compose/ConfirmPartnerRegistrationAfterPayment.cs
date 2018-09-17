using EventRegistrar.Backend.Infrastructure.Events;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Registrations.Confirmation;

namespace EventRegistrar.Backend.Mailing.Compose
{
    public class ConfirmPartnerRegistrationAfterPayment : IEventToCommandTranslation<PartnerRegistrationPaid>
    {
        public IQueueBoundMessage Translate(PartnerRegistrationPaid @event)
        {
            return new ComposeAndSendMailCommand { MailType = MailType.PartnerRegistrationFullyPaid, RegistrationId = @event.RegistrationId1, Withhold = true };
        }
    }
}