using System.Collections.Generic;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Registrations.Cancel;

namespace EventRegistrar.Backend.Mailing.Compose
{
    public class SendMailAfterCancellation : IEventToCommandTranslation<RegistrationCancelled>
    {
        public IEnumerable<IQueueBoundMessage> Translate(RegistrationCancelled e)
        {
            yield return new ComposeAndSendMailCommand
            {
                MailType = MailType.RegistrationCancelled,
                Withhold = true,
                RegistrationId = e.RegistrationId
            };
        }
    }
}