using System.Collections.Generic;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.Confirmation;
using MediatR;

namespace EventRegistrar.Backend.Mailing.Compose
{
    public class SendMailAfterFirstPaymentInPartnerRegistration : IEventToCommandTranslation<PartnerRegistrationPartiallyPaid>
    {
        public IEnumerable<IRequest> Translate(PartnerRegistrationPartiallyPaid e)
        {
            yield return new ComposeAndSendMailCommand
            {
                MailType = MailType.PartnerRegistrationFirstPaid,
                RegistrationId = e.RegistrationId1,
                //Withhold = false
            };
        }
    }
}