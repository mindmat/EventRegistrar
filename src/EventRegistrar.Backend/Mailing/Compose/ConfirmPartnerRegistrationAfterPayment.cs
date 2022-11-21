using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.Confirmation;

namespace EventRegistrar.Backend.Mailing.Compose;

public class ConfirmPartnerRegistrationAfterPayment : IEventToCommandTranslation<PartnerRegistrationPaid>
{
    public IEnumerable<IRequest> Translate(PartnerRegistrationPaid e)
    {
        if (e.EventId != null)
        {
            yield return new ComposeAndSendAutoMailCommand
                         {
                             EventId = e.EventId.Value,
                             MailType = MailType.PartnerRegistrationFullyPaid,
                             RegistrationId = e.RegistrationId1,
                             Withhold = e.WillPayAtCheckin
                         };
        }
    }
}