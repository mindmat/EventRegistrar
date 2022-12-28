using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Registrations.ReadModels;

namespace EventRegistrar.Backend.Registrations.Matching;

public class UnbindPartnerRegistrationCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class UnbindPartnerRegistrationCommandHandler : AsyncRequestHandler<UnbindPartnerRegistrationCommand>
{
    private readonly IRepository<Registration> _registrations;
    private readonly ReadModelUpdater _readModelUpdater;

    public UnbindPartnerRegistrationCommandHandler(IRepository<Registration> registrations,
                                                   ReadModelUpdater readModelUpdater)
    {
        _registrations = registrations;
        _readModelUpdater = readModelUpdater;
    }

    protected override async Task Handle(UnbindPartnerRegistrationCommand command, CancellationToken cancellationToken)
    {
        var registration = await _registrations.AsTracking()
                                               .Include(reg => reg.Seats_AsLeader)
                                               .Include(reg => reg.Seats_AsFollower)
                                               .FirstAsync(reg => reg.Id == command.RegistrationId
                                                               && reg.EventId == command.EventId, cancellationToken);

        Guid? registrationId_Partner = null;
        if (registration.RegistrationId_Partner != null)
        {
            registrationId_Partner = registration.RegistrationId_Partner;
            registration.RegistrationId_Partner = null;
        }

        // unbind spots
        if (registration.Seats_AsLeader != null)
        {
            foreach (var spot in registration.Seats_AsLeader.Where(spot => spot.IsPartnerSpot))
            {
                spot.IsPartnerSpot = false;
            }
        }

        if (registration.Seats_AsFollower != null)
        {
            foreach (var spot in registration.Seats_AsFollower.Where(spot => spot.IsPartnerSpot))
            {
                spot.IsPartnerSpot = false;
            }
        }

        _readModelUpdater.TriggerUpdate<RegistrationCalculator>(command.EventId, registration.Id);
        if (registrationId_Partner != null)
        {
            _readModelUpdater.TriggerUpdate<RegistrationCalculator>(command.EventId, registrationId_Partner);
        }
    }
}