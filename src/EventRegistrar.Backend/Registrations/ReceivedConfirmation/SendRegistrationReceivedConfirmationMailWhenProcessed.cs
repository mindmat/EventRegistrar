using EventRegistrar.Backend.Events.Context;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Registrations.Register;

namespace EventRegistrar.Backend.Registrations.ReceivedConfirmation;

internal class SendRegistrationReceivedConfirmationMailWhenProcessed : IEventToCommandTranslation<RegistrationProcessed>
{
    private readonly MailReleaseConfiguration _configuration;
    private readonly EventContext _eventContext;

    public SendRegistrationReceivedConfirmationMailWhenProcessed(MailReleaseConfiguration configuration,
                                                                 EventContext eventContext)
    {
        _configuration = configuration;
        _eventContext = eventContext;
    }

    public IEnumerable<IRequest> Translate(RegistrationProcessed e)
    {
        if (_configuration.SendRegistrationReceivedMail)
        {
            var eventId = _eventContext.EventId ?? e.EventId;
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