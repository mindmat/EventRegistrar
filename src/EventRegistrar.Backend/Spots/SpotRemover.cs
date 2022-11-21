using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Spots;

public class SpotRemover
{
    private readonly IEventBus _eventBus;
    private readonly IQueryable<Registration> _registrations;
    private readonly IQueryable<Registrable> _registrables;

    public SpotRemover(IEventBus eventBus,
                       IQueryable<Registration> registrations,
                       IQueryable<Registrable> registrables)
    {
        _eventBus = eventBus;
        _registrations = registrations;
        _registrables = registrables;
    }

    public void RemoveSpot(Seat spot, Guid registrationId, RemoveSpotReason reason)
    {
        if (spot.RegistrationId == registrationId)
        {
            if (spot.RegistrationId_Follower.HasValue)
            {
                // double spot, leave the partner in
                spot.RegistrationId = null;
                spot.PartnerEmail = null;
                spot.IsPartnerSpot = false;
            }
            else
            {
                // single spot, cancel the place
                spot.IsCancelled = true;
            }
        }
        else if (spot.RegistrationId_Follower == registrationId)
        {
            if (spot.RegistrationId.HasValue)
            {
                // double spot, leave the partner in
                spot.RegistrationId_Follower = null;
                spot.PartnerEmail = null;
                spot.IsPartnerSpot = false;
            }
            else
            {
                // single spot, cancel the place
                spot.IsCancelled = true;
            }
        }

        var registration = _registrations.First(reg => reg.Id == registrationId);
        var registrable = _registrables.First(rbl => rbl.Id == spot.RegistrableId);
        _eventBus.Publish(new SpotRemoved
                          {
                              Id = Guid.NewGuid(),
                              RegistrableId = spot.RegistrableId,
                              RegistrationId = registrationId,
                              Reason = reason,
                              WasSpotOnWaitingList = spot.IsWaitingList,
                              Participant = $"{registration.RespondentFirstName} {registration.RespondentLastName}",
                              Registrable = registrable.DisplayName
                          });
    }
}