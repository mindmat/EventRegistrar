using EventRegistrar.Backend.Infrastructure.Events;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Registrations.Confirmation;

namespace EventRegistrar.Backend.Mailing.Compose
{
    public class SendMailAfterFirstPaymentInPartnerRegistration : IEventToCommandTranslation<PartnerRegistrationPartiallyPaid>
    {
        public IQueueBoundMessage Translate(PartnerRegistrationPartiallyPaid @event)
        {
            return new ComposeAndSendMailCommand { MailType = MailType.PartnerRegistrationFirstPaid, RegistrationId = @event.RegistrationId1, Withhold = true };
        }
    }
}