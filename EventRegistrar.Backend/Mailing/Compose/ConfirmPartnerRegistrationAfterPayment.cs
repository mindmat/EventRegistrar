using System.Collections.Generic;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Registrations.Confirmation;

namespace EventRegistrar.Backend.Mailing.Compose
{
    public class ConfirmPartnerRegistrationAfterPayment : IEventToCommandTranslation<PartnerRegistrationPaid>
    {
        public IEnumerable<IQueueBoundMessage> Translate(PartnerRegistrationPaid e)
        {
            yield return new ComposeAndSendMailCommand
            {
                MailType = MailType.PartnerRegistrationFullyPaid,
                RegistrationId = e.RegistrationId1,
                Withhold = true
            };
        }
    }
}