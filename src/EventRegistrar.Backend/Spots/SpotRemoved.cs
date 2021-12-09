using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Spots;

public class SpotRemoved : DomainEvent
{
    public RemoveSpotReason Reason { get; set; }
    public Guid RegistrableId { get; set; }
    public Guid RegistrationId { get; set; }
    public bool WasSpotOnWaitingList { get; set; }
    public string Participant { get; set; }
    public string Registrable { get; set; }
}

public class SpotRemovedUserTranslation : IEventToUserTranslation<SpotRemoved>
{
    public string GetText(SpotRemoved domainEvent)
    {
        return
            $"{domainEvent.Participant} wurde aus {(domainEvent.WasSpotOnWaitingList ? "der Warteliste von " : "")}{domainEvent.Registrable} entfernt.";
    }
}