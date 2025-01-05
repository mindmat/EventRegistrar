using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Registrables;

namespace EventRegistrar.Backend.Registrations.Matching;

public class UnbindPartnerRegistrationCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
    public bool LeavePartnerSpots { get; set; }
}

public class UnbindPartnerRegistrationCommandHandler(IRepository<Registration> registrations,
                                                     ChangeTrigger changeTrigger)
    : IRequestHandler<UnbindPartnerRegistrationCommand>
{
    public async Task Handle(UnbindPartnerRegistrationCommand command, CancellationToken cancellationToken)
    {
        var registrationId_Partner = await UnbindRegistration(command.EventId, 
                                                              command.RegistrationId, 
                                                              command.LeavePartnerSpots, 
                                                              cancellationToken);
        if (registrationId_Partner != null)
        {
            await UnbindRegistration(command.EventId, 
                                     registrationId_Partner.Value, 
                                     command.LeavePartnerSpots, 
                                     cancellationToken);
        }
    }

    private async Task<Guid?> UnbindRegistration(Guid eventId, Guid registrationId, bool leavePartnerSpots, CancellationToken cancellationToken)
    {
        var registration = await registrations.AsTracking()
                                              .Include(reg => reg.Seats_AsLeader)
                                              .Include(reg => reg.Seats_AsFollower)
                                              .FirstAsync(reg => reg.Id == registrationId
                                                              && reg.EventId == eventId, cancellationToken);

        Guid? registrationId_Partner = null;
        if (registration.RegistrationId_Partner != null)
        {
            registrationId_Partner = registration.RegistrationId_Partner;
            registration.RegistrationId_Partner = null;
        }

        registration.PartnerOriginal = null;
        registration.PartnerNormalized = null;

        if (!leavePartnerSpots)
        {
            // unbind spots
            if (registration.Seats_AsLeader != null)
            {
                foreach (var spot in registration.Seats_AsLeader.Where(spot => spot.IsPartnerSpot))
                {
                    spot.IsPartnerSpot = false;
                    spot.PartnerEmail = null;
                }
            }

            if (registration.Seats_AsFollower != null)
            {
                foreach (var spot in registration.Seats_AsFollower.Where(spot => spot.IsPartnerSpot))
                {
                    spot.IsPartnerSpot = false;
                    spot.PartnerEmail = null;
                }
            }
        }

        changeTrigger.TriggerUpdate<RegistrablesOverviewCalculator>(null, eventId);
        changeTrigger.TriggerUpdate<RegistrationCalculator>(registration.Id, eventId);
        changeTrigger.TriggerUpdate<RegistrationsWithUnmatchedPartnerCalculator>(null, eventId);
        return registrationId_Partner;
    }
}