using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrations.Matching;

public class ChangeUnmatchedPartnerRegistrationToSingleRegistrationCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class ChangeUnmatchedPartnerRegistrationToSingleRegistrationCommandHandler : IRequestHandler<ChangeUnmatchedPartnerRegistrationToSingleRegistrationCommand>
{
    private readonly IRepository<Registration> _registrations;
    private readonly IRepository<Seat> _spots;

    public ChangeUnmatchedPartnerRegistrationToSingleRegistrationCommandHandler(IRepository<Registration> registrations,
                                                                                IRepository<Seat> spots)
    {
        _registrations = registrations;
        _spots = spots;
    }

    public async Task<Unit> Handle(ChangeUnmatchedPartnerRegistrationToSingleRegistrationCommand command, CancellationToken cancellationToken)
    {
        var registration = await _registrations.Where(reg => reg.EventId == command.EventId
                                                          && reg.Id == command.RegistrationId)
                                               .Include(reg => reg.Seats_AsFollower)
                                               .Include(reg => reg.Seats_AsLeader)
                                               .FirstAsync(cancellationToken);

        if (registration.RegistrationId_Partner != null)
        {
            throw new ArgumentException($"Registration {registration.Id} is already assigned to a partner");
        }

        if (registration.RegistrationId_Partner != null)
        {
            throw new ArgumentException($"Registration {registration.Id} is already assigned to a partner");
        }

        var partnerRegistration = await _registrations.FirstOrDefaultAsync(reg => reg.EventId == command.EventId
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
            await _spots.InsertOrUpdateEntity(spot, cancellationToken);
        }

        foreach (var spot in registration.Seats_AsFollower!.Where(spt => !spt.IsCancelled && spt.IsUnmatchedPartnerSpot()))
        {
            spot.IsPartnerSpot = false;
            spot.PartnerEmail = null;
            await _spots.InsertOrUpdateEntity(spot, cancellationToken);
        }

        registration.RegistrationId_Partner = null;
        registration.PartnerNormalized = null;
        await _registrations.InsertOrUpdateEntity(registration, cancellationToken);

        return Unit.Value;
    }
}