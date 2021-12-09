using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations.Cancel;
using MediatR;

namespace EventRegistrar.Backend.Mailing.Compose;

public class SendMailAfterCancellation : IEventToCommandTranslation<RegistrationCancelled>
{
    public IEnumerable<IRequest> Translate(RegistrationCancelled e)
    {
        yield return new ComposeAndSendMailCommand
                     {
                         MailType = MailType.RegistrationCancelled,
                         //Withhold = true,
                         RegistrationId = e.RegistrationId
                     };
    }
}