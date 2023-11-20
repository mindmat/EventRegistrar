using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Payments.PayAtCheckin;

public class WillPayAtCheckinSet : DomainEvent
{
    public Guid RegistrationId { get; set; }
}

public class WillPayAtCheckinSetUserTranslation(IQueryable<Registration> registrations) : IEventToUserTranslation<WillPayAtCheckinSet>
{
    public string GetText(WillPayAtCheckinSet domainEvent)
    {
        var registration = registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);
        return $"{registration?.RespondentFirstName} {registration?.RespondentLastName} wird am Checkin bezahlen";
    }
}