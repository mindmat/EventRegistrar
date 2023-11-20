using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Mailing.InvalidAddresses;

public class InvalidEmailAddressFixed : DomainEvent
{
    public string NewEmailAddress { get; set; } = null!;
    public string? OldEmailAddress { get; set; }
    public Guid RegistrationId { get; set; }
}

public class InvalidEmailAddressFixedUserTranslation(IQueryable<Registration> registrations) : IEventToUserTranslation<InvalidEmailAddressFixed>
{
    public string GetText(InvalidEmailAddressFixed domainEvent)
    {
        var registration = registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);
        return
            $"Ungültige Mailadresse {domainEvent.OldEmailAddress} zu {domainEvent.NewEmailAddress} geändert (Anmeldung {registration?.RespondentFirstName} {registration?.RespondentLastName})";
    }
}