using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Registrables.WaitingList;

public class PartnerSpotMovedUpFromWaitingList : DomainEvent
{
    public Guid RegistrableId { get; set; }
    public Guid? RegistrationId { get; set; }
    public Guid? RegistrationId_Follower { get; set; }
}

public class PartnerSpotMovedUpFromWaitingListUserTranslation : IEventToUserTranslation<PartnerSpotMovedUpFromWaitingList>
{
    private readonly IQueryable<Registration> _registrations;
    private readonly IQueryable<Registrable> _registrables;

    public PartnerSpotMovedUpFromWaitingListUserTranslation(IQueryable<Registration> registrations,
                                                            IQueryable<Registrable> registrables)
    {
        _registrations = registrations;
        _registrables = registrables;
    }

    public string GetText(PartnerSpotMovedUpFromWaitingList domainEvent)
    {
        var registrationLeader = _registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId);
        var registrationFollower = _registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId_Follower);
        var registrable = _registrables.FirstOrDefault(reg => reg.Id == domainEvent.RegistrableId);
        return
            $"{registrationLeader?.RespondentFirstName} {registrationLeader?.RespondentLastName} und {registrationFollower?.RespondentFirstName} {registrationFollower?.RespondentLastName} sind in {registrable?.DisplayName} von der Warteliste nachgerückt";
    }
}