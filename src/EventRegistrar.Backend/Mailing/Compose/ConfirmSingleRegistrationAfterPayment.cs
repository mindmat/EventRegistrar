using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.Confirmation;

namespace EventRegistrar.Backend.Mailing.Compose;

public class ConfirmSingleRegistrationAfterPayment : IEventToCommandTranslation<SingleRegistrationPaid>
{
    public IEnumerable<IRequest> Translate(SingleRegistrationPaid e)
    {
        if (e.EventId != null)
        {
            yield return new ComposeAndSendAutoMailCommand
                         {
                             EventId = e.EventId.Value,
                             MailType = MailType.SingleRegistrationFullyPaid,
                             RegistrationId = e.RegistrationId,
                             Withhold = e.WillPayAtCheckin
                         };
        }
    }
}