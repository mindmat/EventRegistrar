using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Payments.PayAtCheckin;

public class WillPayAtCheckinSet : DomainEvent
{
    public Guid RegistrationId { get; set; }
    public bool WillPayAtCheckin { get; set; }
}

public class WillPayAtCheckinSetUserTranslation(IQueryable<Registration> registrations) : IEventToUserTranslation<WillPayAtCheckinSet>
{
    public string GetText(WillPayAtCheckinSet domainEvent)
    {
        var registration = registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);
        var nicht = domainEvent.WillPayAtCheckin ? string.Empty : "nicht ";
        return $"{registration?.RespondentFirstName} {registration?.RespondentLastName} wird {nicht}am Checkin bezahlen";
    }
}