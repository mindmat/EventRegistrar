using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.Confirmation;
using MediatR;

namespace EventRegistrar.Backend.Mailing.Compose;

public class ConfirmPartnerRegistrationAfterPayment : IEventToCommandTranslation<PartnerRegistrationPaid>
{
    public IEnumerable<IRequest> Translate(PartnerRegistrationPaid e)
    {
        yield return new ComposeAndSendMailCommand
                     {
                         MailType = MailType.PartnerRegistrationFullyPaid,
                         RegistrationId = e.RegistrationId1,
                         Withhold = e.WillPayAtCheckin
                     };
    }
}