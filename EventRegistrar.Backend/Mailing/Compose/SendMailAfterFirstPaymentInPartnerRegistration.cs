using EventRegistrar.Backend.Infrastructure.Events;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Registrations.Confirmation;

namespace EventRegistrar.Backend.Mailing.Compose
{
    public class SendMailAfterFirstPaymentInPartnerRegistration : IEventToCommandTranslation<PartnerRegistrationPartiallyPaid>
    {
        public IQueueBoundMessage Translate(PartnerRegistrationPartiallyPaid e)
        {
            return new ComposeAndSendMailCommand { MailType = MailType.PartnerRegistrationFirstPaid, RegistrationId = e.RegistrationId1, Withhold = true };
        }
    }
}