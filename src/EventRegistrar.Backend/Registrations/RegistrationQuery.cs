using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Registrations.ReadModels;

using MediatR;

namespace EventRegistrar.Backend.Registrations;

public class RegistrationQuery : IRequest<RegistrationDisplayItem?>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class RegistrationQueryHandler : IRequestHandler<RegistrationQuery, RegistrationDisplayItem?>
{
    private readonly IQueryable<RegistrationQueryReadModel> _registrations;

    public RegistrationQueryHandler(IQueryable<RegistrationQueryReadModel> registrations)
    {
        _registrations = registrations;
    }

    public async Task<RegistrationDisplayItem?> Handle(RegistrationQuery query, CancellationToken cancellationToken)
    {
        var registration = await _registrations.Where(reg => reg.EventId == query.EventId
                                                          && reg.RegistrationId == query.RegistrationId)
                                               .Select(reg => reg.Content)
                                               .FirstAsync(cancellationToken);
        return registration;
    }
}