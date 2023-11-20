using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrations.Cancel;

public class RegistrationCancelled : DomainEvent
{
    public Guid RegistrationId { get; set; }
    public string? Reason { get; set; }
    public decimal Refund { get; set; }
    public DateTimeOffset Received { get; set; }
    public string? Participant { get; set; }
}

public class RegistrationCancelledUserTranslation(IQueryable<Registration> registrations) : IEventToUserTranslation<RegistrationCancelled>
{
    public string GetText(RegistrationCancelled domainEvent)
    {
        var registration = registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);
        return
            $"{registration?.RespondentFirstName} {registration?.RespondentLastName} hat am {domainEvent.Received:g} storniert mit Begründung '{domainEvent.Reason}'. Rückerstattung {domainEvent.Refund}";
    }
}