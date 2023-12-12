using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Registrations.ReadModels;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations.Matching;

public class ChangeUnmatchedPartnerRegistrationToSingleRegistrationCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class ChangeUnmatchedPartnerRegistrationToSingleRegistrationCommandHandler(IRepository<Registration> registrations,
                                                                                  IRepository<Seat> spots,
                                                                                  ChangeTrigger changeTrigger)
    : IRequestHandler<ChangeUnmatchedPartnerRegistrationToSingleRegistrationCommand>
{
    public async Task Handle(ChangeUnmatchedPartnerRegistrationToSingleRegistrationCommand command, CancellationToken cancellationToken)
    {
        var registration = await registrations.AsTracking()
                                              .Where(reg => reg.EventId == command.EventId
                                                         && reg.Id == command.RegistrationId)
                                              .Include(reg => reg.Seats_AsFollower)
                                              .Include(reg => reg.Seats_AsLeader)
                                              .FirstAsync(cancellationToken);

        if (registration.RegistrationId_Partner != null)
        {
            throw new ArgumentException($"Registration {registration.Id} is already assigned to a partner");
        }

        var partnerRegistration = await registrations.FirstOrDefaultAsync(reg => reg.EventId == command.EventId
                                                                              && reg.RegistrationId_Partner == registration.Id
                                                                              && reg.State != RegistrationState.Cancelled, cancellationToken);
        if (partnerRegistration != null)
        {
            throw new ArgumentException(
                $"Registration {registration.Id} is referenced by registration {partnerRegistration.Id} ({registration.RespondentFirstName} {registration.RespondentLastName})");
        }

        if (registration.Seats_AsLeader!.Any(spt => !spt.IsCancelled && spt.IsMatchedPartnerSpot())
         || registration.Seats_AsFollower!.Any(spt => !spt.IsCancelled && spt.IsMatchedPartnerSpot()))
        {
            throw new ArgumentException($"Unexpected situation: registration {registration.Id} has partner spot with a follower set");
        }

        // ok, everything seems to be fine, let's change to single registration
        foreach (var spot in registration.Seats_AsLeader!.Where(spt => !spt.IsCancelled && spt.IsUnmatchedPartnerSpot()))
        {
            spot.IsPartnerSpot = false;
            spot.PartnerEmail = null;
            await spots.InsertOrUpdateEntity(spot, cancellationToken);
        }

        foreach (var spot in registration.Seats_AsFollower!.Where(spt => !spt.IsCancelled && spt.IsUnmatchedPartnerSpot()))
        {
            spot.IsPartnerSpot = false;
            spot.PartnerEmail = null;
            await spots.InsertOrUpdateEntity(spot, cancellationToken);
        }

        registration.RegistrationId_Partner = null;
        registration.PartnerOriginal = null;
        registration.PartnerNormalized = null;

        changeTrigger.QueryChanged<RegistrationsWithUnmatchedPartnerQuery>(command.EventId);
        changeTrigger.TriggerUpdate<RegistrationCalculator>(registration.Id, command.EventId);
    }
}