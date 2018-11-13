using System.Collections.Generic;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Registrations.Confirmation;

namespace EventRegistrar.Backend.Mailing.Compose
{
    public class ConfirmSingleRegistrationAfterPayment : IEventToCommandTranslation<SingleRegistrationPaid>
    {
        public IEnumerable<IQueueBoundMessage> Translate(SingleRegistrationPaid e)
        {
            yield return new ComposeAndSendMailCommand
            {
                MailType = MailType.SingleRegistrationFullyPaid,
                RegistrationId = e.RegistrationId,
                Withhold = true
            };
        }
    }
}