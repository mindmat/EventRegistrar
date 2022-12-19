using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Mailing.InvalidAddresses;

public class InvalidEmailAddressFixed : DomainEvent
{
    public string NewEmailAddress { get; set; } = null!;
    public string? OldEmailAddress { get; set; }
    public Guid RegistrationId { get; set; }
}

public class InvalidEmailAddressFixedUserTranslation : IEventToUserTranslation<InvalidEmailAddressFixed>
{
    private readonly IQueryable<Registration> _registrations;

    public InvalidEmailAddressFixedUserTranslation(IQueryable<Registration> registrations)
    {
        _registrations = registrations;
    }

    public string GetText(InvalidEmailAddressFixed domainEvent)
    {
        var registration = _registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);
        return
            $"Ungültige Mailadresse {domainEvent.OldEmailAddress} zu {domainEvent.NewEmailAddress} geändert (Anmeldung {registration?.RespondentFirstName} {registration?.RespondentLastName})";
    }
}