using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Registrables.WaitingList;

public class RegistrationMovedUpFromWaitingList : DomainEvent
{
    public Guid? RegistrationId { get; set; }
}

public class RegistrationMovedUpFromWaitingListUserTranslation(IQueryable<Registration> registrations) : IEventToUserTranslation<RegistrationMovedUpFromWaitingList>
{
    public string GetText(RegistrationMovedUpFromWaitingList domainEvent)
    {
        var registration = registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);
        return $"{registration?.RespondentFirstName} {registration?.RespondentLastName} ist nicht mehr auf der Warteliste";
    }
}