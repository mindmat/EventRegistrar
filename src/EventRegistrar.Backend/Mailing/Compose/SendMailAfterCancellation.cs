using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.Cancel;

namespace EventRegistrar.Backend.Mailing.Compose;

public class SendMailAfterCancellation : IEventToCommandTranslation<RegistrationCancelled>
{
    public IEnumerable<IRequest> Translate(RegistrationCancelled e)
    {
        if (e.EventId != null)
        {
            yield return new ComposeAndSendAutoMailCommand
                         {
                             EventId = e.EventId.Value,
                             MailType = MailType.RegistrationCancelled,
                             RegistrationId = e.RegistrationId
                         };
        }
    }
}