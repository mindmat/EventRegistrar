using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.PhoneMessages;

public class SmsSent : DomainEvent
{
    public Guid? RegistrationId { get; set; }
    public string To { get; set; }
    public string Text { get; set; }
    public DateTimeOffset Sent { get; set; }
}

public class SmsSentUserTranslation(IQueryable<Registration> registrations) : IEventToUserTranslation<SmsSent>
{
    public string GetText(SmsSent domainEvent)
    {
        var registration = registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);
        return
            $"SMS gesendet an {registration?.RespondentFirstName} {registration?.RespondentLastName} ({domainEvent.To}): \"{domainEvent.Text}\"";
    }
}