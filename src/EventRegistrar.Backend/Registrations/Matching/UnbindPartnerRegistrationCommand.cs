using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;

namespace EventRegistrar.Backend.Registrations.Matching;

public class UnbindPartnerRegistrationCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class UnbindPartnerRegistrationCommandHandler : IRequestHandler<UnbindPartnerRegistrationCommand>
{
    private readonly IRepository<Registration> _registrations;

    public UnbindPartnerRegistrationCommandHandler(IRepository<Registration> registrations)
    {
        _registrations = registrations;
    }

    public async Task<Unit> Handle(UnbindPartnerRegistrationCommand command, CancellationToken cancellationToken)
    {
        var registration = await _registrations.Include(reg => reg.Seats_AsLeader)
                                               .Include(reg => reg.Seats_AsFollower)
                                               .FirstAsync(reg => reg.Id == command.RegistrationId
                                                               && reg.EventId == command.EventId, cancellationToken);

        if (registration.RegistrationId_Partner != null) registration.RegistrationId_Partner = null;

        // unbind spots
        if (registration.Seats_AsLeader != null)
            foreach (var spot in registration.Seats_AsLeader.Where(spot => spot.IsPartnerSpot))
                spot.IsPartnerSpot = false;
        if (registration.Seats_AsFollower != null)
            foreach (var spot in registration.Seats_AsFollower.Where(spot => spot.IsPartnerSpot))
                spot.IsPartnerSpot = false;

        return Unit.Value;
    }
}