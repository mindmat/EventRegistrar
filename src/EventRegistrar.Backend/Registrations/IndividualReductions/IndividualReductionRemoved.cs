using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrations.IndividualReductions;

public class IndividualReductionRemoved : DomainEvent
{
    public decimal Amount { get; set; }
    public Guid RegistrationId { get; set; }
    public string? Reason { get; set; }
}

public class IndividualReductionRemovedUserTranslation : IEventToUserTranslation<IndividualReductionRemoved>
{
    private readonly IQueryable<Registration> _registrations;

    public IndividualReductionRemovedUserTranslation(IQueryable<Registration> registrations)
    {
        _registrations = registrations;
    }

    public string GetText(IndividualReductionRemoved domainEvent)
    {
        var registration = _registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);
        return $"Bei {registration?.RespondentFirstName} {registration?.RespondentLastName} wurde ein persönlicher Rabatt über {domainEvent.Amount} entfernt.";
    }
}