using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Registrables.WaitingList;

public class RegistrationMovedUpFromWaitingList : DomainEvent
{
    public Guid? RegistrationId { get; set; }
}

public class RegistrationMovedUpFromWaitingListUserTranslation : IEventToUserTranslation<RegistrationMovedUpFromWaitingList>
{
    private readonly IQueryable<Registration> _registrations;

    public RegistrationMovedUpFromWaitingListUserTranslation(IQueryable<Registration> registrations)
    {
        _registrations = registrations;
    }

    public string GetText(RegistrationMovedUpFromWaitingList domainEvent)
    {
        var registration = _registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);
        return $"{registration?.RespondentFirstName} {registration?.RespondentLastName} ist nicht mehr auf der Warteliste";
    }
}