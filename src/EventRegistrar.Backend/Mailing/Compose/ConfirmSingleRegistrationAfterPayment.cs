using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.Confirmation;
using MediatR;

namespace EventRegistrar.Backend.Mailing.Compose;

public class ConfirmSingleRegistrationAfterPayment : IEventToCommandTranslation<SingleRegistrationPaid>
{
    public IEnumerable<IRequest> Translate(SingleRegistrationPaid e)
    {
        yield return new ComposeAndSendMailCommand
                     {
                         MailType = MailType.SingleRegistrationFullyPaid,
                         RegistrationId = e.RegistrationId,
                         Withhold = e.WillPayAtCheckin
                     };
    }
}