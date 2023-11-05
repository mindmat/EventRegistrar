using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Registrables;
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
    private readonly ChangeTrigger _changeTrigger;

    public UnbindPartnerRegistrationCommandHandler(IRepository<Registration> registrations,
                                                   ChangeTrigger changeTrigger)
    {
        _registrations = registrations;
        _changeTrigger = changeTrigger;
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

        registration.PartnerOriginal = null;
        registration.PartnerNormalized = null;

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

        _changeTrigger.TriggerUpdate<RegistrablesOverviewCalculator>(null, command.EventId);
        _changeTrigger.TriggerUpdate<RegistrationCalculator>(registration.Id, command.EventId);
        if (registrationId_Partner != null)
        {
            _changeTrigger.TriggerUpdate<RegistrationCalculator>(registrationId_Partner, command.EventId);
        }
    }
}