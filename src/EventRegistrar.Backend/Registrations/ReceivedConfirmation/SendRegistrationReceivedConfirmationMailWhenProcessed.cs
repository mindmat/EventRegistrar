using EventRegistrar.Backend.Events.Context;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Registrations.Register;

namespace EventRegistrar.Backend.Registrations.ReceivedConfirmation;

internal class SendRegistrationReceivedConfirmationMailWhenProcessed(MailReleaseConfiguration configuration,
                                                                     EventContext eventContext)
    : IEventToCommandTranslation<RegistrationProcessed>
{
    public IEnumerable<IRequest> Translate(RegistrationProcessed e)
    {
        if (configuration.SendRegistrationReceivedMail)
        {
            var eventId = eventContext.EventId ?? e.EventId;
            if (eventId != null)
            {
                yield return new ComposeAndSendAutoMailCommand
                             {
                                 EventId = eventId.Value,
                                 MailType = MailType.RegistrationReceived,
                                 RegistrationId = e.RegistrationId
                             };
            }
        }
    }
}