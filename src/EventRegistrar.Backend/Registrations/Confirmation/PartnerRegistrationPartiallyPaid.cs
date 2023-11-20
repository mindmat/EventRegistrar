using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Registrations.Confirmation;

public class PartnerRegistrationPartiallyPaid : DomainEvent
{
    public Guid RegistrationId1 { get; set; }
    public Guid RegistrationId2 { get; set; }
}

public class PartnerRegistrationPartiallyPaidUserTranslation(IQueryable<Registration> registrations) : IEventToUserTranslation<PartnerRegistrationPartiallyPaid>
{
    public string GetText(PartnerRegistrationPartiallyPaid domainEvent)
    {
        var registrationLeader = registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId1);
        var registrationFollower = registrations.FirstOrDefault(reg => reg.Id == domainEvent.RegistrationId2);
        return
            $"{registrationLeader?.RespondentFirstName} {registrationLeader?.RespondentLastName} und {registrationFollower?.RespondentFirstName} {registrationFollower?.RespondentLastName} haben teilweise bezahlt";
    }
}