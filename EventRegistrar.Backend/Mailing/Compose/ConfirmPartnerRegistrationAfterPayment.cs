using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Registrations.Confirmation;

namespace EventRegistrar.Backend.Mailing.Compose
{
    public class ConfirmPartnerRegistrationAfterPayment : IEventToCommandTranslation<PartnerRegistrationPaid>
    {
        public IQueueBoundMessage Translate(PartnerRegistrationPaid e)
        {
            return new ComposeAndSendMailCommand { MailType = MailType.PartnerRegistrationFullyPaid, RegistrationId = e.RegistrationId1, Withhold = true };
        }
    }
}