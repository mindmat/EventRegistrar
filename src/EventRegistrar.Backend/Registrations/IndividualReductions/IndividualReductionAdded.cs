using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrations.IndividualReductions;

public class IndividualReductionAdded : DomainEvent
{
    public decimal Amount { get; set; }
    public Guid RegistrationId { get; set; }
    public string? Reason { get; set; }
}

public class IndividualReductionAddedUserTranslation(IQueryable<Registration> registrations) : IEventToUserTranslation<IndividualReductionAdded>
{
    public string GetText(IndividualReductionAdded domainEvent)
    {
        var registration = registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);
        return
            $"{registration?.RespondentFirstName} {registration?.RespondentLastName} wurde ein persönlicher Rabatt über {domainEvent.Amount} gewährt. Begründung: {domainEvent.Reason}";
    }
}