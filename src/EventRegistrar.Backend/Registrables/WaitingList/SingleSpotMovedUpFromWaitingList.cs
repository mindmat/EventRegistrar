using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Registrables.WaitingList;

public class SingleSpotMovedUpFromWaitingList : DomainEvent
{
    public Guid RegistrableId { get; set; }
    public Guid RegistrationId { get; set; }
    public Role Role { get; set; }
    public string Registrable { get; set; }
    public string Participant { get; set; }
}

public class SingleSpotMovedUpFromWaitingListUserTranslation : IEventToUserTranslation<SingleSpotMovedUpFromWaitingList>
{
    public string GetText(SingleSpotMovedUpFromWaitingList domainEvent)
    {
        return $"{domainEvent.Participant} ist von der Warteliste von {domainEvent.Registrable} nachgerückt.";
    }
}