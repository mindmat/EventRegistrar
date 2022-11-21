using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.Confirmation;

namespace EventRegistrar.Backend.Mailing.Compose;

public class
    SendMailAfterFirstPaymentInPartnerRegistration : IEventToCommandTranslation<PartnerRegistrationPartiallyPaid>
{
    public IEnumerable<IRequest> Translate(PartnerRegistrationPartiallyPaid e)
    {
        if (e.EventId != null)
        {
            yield return new ComposeAndSendAutoMailCommand
                         {
                             EventId = e.EventId.Value,
                             MailType = MailType.PartnerRegistrationFirstPaid,
                             RegistrationId = e.RegistrationId1
                         };
        }
    }
}