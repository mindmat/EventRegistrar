using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Payments.PayAtCheckin;

public class WillPayAtCheckinSet : DomainEvent
{
    public Guid RegistrationId { get; set; }
}

public class WillPayAtCheckinSetUserTranslation : IEventToUserTranslation<WillPayAtCheckinSet>
{
    private readonly IQueryable<Registration> _registrations;

    public WillPayAtCheckinSetUserTranslation(IQueryable<Registration> registrations)
    {
        _registrations = registrations;
    }

    public string GetText(WillPayAtCheckinSet domainEvent)
    {
        var registration = _registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);
        return $"{registration?.RespondentFirstName} {registration?.RespondentLastName} wird am Checkin bezahlen";
    }
}