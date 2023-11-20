using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Spots;

public class SpotAdded : DomainEvent
{
    public bool IsInitialProcessing { get; set; }
    public Guid RegistrableId { get; set; }
    public Guid RegistrationId { get; set; }
    public string Registrable { get; set; }
    public string Participant { get; set; }
    public bool IsWaitingList { get; set; }
}

public class SpotAddedUserTranslation(IQueryable<Registration> registrations) : IEventToUserTranslation<SpotAdded>
{
    public string GetText(SpotAdded domainEvent)
    {
        var registration = registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);
        return
            $"{registration?.RespondentFirstName} {registration?.RespondentLastName} wurde in {(domainEvent.IsWaitingList ? "die Warteliste von " : "")}{domainEvent.Registrable} aufgenommen.";
    }
}