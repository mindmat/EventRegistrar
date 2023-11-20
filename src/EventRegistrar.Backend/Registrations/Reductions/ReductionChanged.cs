using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrations.Reductions;

public class ReductionChanged : DomainEvent
{
    public bool IsReduced { get; set; }
    public Guid RegistrationId { get; set; }
}

public class ReductionChangedUserTranslation(IQueryable<Registration> registrations) : IEventToUserTranslation<ReductionChanged>
{
    public string GetText(ReductionChanged domainEvent)
    {
        var registration = registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);
        return domainEvent.IsReduced
                   ? $"{registration?.RespondentFirstName} {registration?.RespondentLastName} hat Anspruch auf eine Reduktion"
                   : $"{registration?.RespondentFirstName} {registration?.RespondentLastName} hat keinen Anspruch auf eine Reduktion";
    }
}