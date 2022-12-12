using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrables;

namespace EventRegistrar.Backend.Registrations.Register;

public class RegistrationProcessed : DomainEvent
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string[]? Registrables { get; set; }
    public Guid RegistrationId { get; set; }
    public string? Email { get; set; }
}

public class RegistrationProcessedUserTranslation : IEventToUserTranslation<RegistrationProcessed>
{
    public string GetText(RegistrationProcessed domainEvent)
    {
        return $"Teilnehmer: {domainEvent.FirstName} {domainEvent.LastName}, angemeldet für {domainEvent.Registrables.StringJoin()}";
    }
}