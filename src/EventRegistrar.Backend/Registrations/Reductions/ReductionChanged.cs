using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrations.Reductions;

public class ReductionChanged : DomainEvent
{
    public bool IsReduced { get; set; }
    public Guid RegistrationId { get; set; }
}

public class ReductionChangedUserTranslation : IEventToUserTranslation<ReductionChanged>
{
    private readonly IQueryable<Registration> _registrations;

    public ReductionChangedUserTranslation(IQueryable<Registration> registrations)
    {
        _registrations = registrations;
    }

    public string GetText(ReductionChanged domainEvent)
    {
        var registration = _registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);
        return domainEvent.IsReduced
            ? $"{registration?.RespondentFirstName} {registration?.RespondentLastName} hat Anspruch auf eine Reduktion"
            : $"{registration?.RespondentFirstName} {registration?.RespondentLastName} hat keinen Anspruch auf eine Reduktion";
    }
}