using EventRegistrar.Backend.Infrastructure.Events;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Registrations.Cancel;

namespace EventRegistrar.Backend.Mailing.Compose
{
    public class SendMailAfterCancellation : IEventToCommandTranslation<RegistrationCancelled>
    {
        public IQueueBoundMessage Translate(RegistrationCancelled e)
        {
            return new ComposeAndSendMailCommand { MailType = MailType.RegistrationCancelled, Withhold = true, RegistrationId = e.RegistrationId };
        }
    }
}