using EventRegistrar.Backend.Events.UsersInEvents;

using MediatR;

namespace EventRegistrar.Backend.Authorization;

public class RightsOfUserInEventQuery : IRequest<IEnumerable<string>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class RightsOfUserInEventQueryHandler : IRequestHandler<RightsOfUserInEventQuery, IEnumerable<string>>
{
    private readonly AuthenticatedUserId _userId;
    private readonly RightsOfUserInEventCache _cache;

    public RightsOfUserInEventQueryHandler(AuthenticatedUserId userId,
                                           RightsOfUserInEventCache cache)
    {
        _userId = userId;
        _cache = cache;
    }

    public async Task<IEnumerable<string>> Handle(RightsOfUserInEventQuery query, CancellationToken cancellationToken)
    {
        return _userId.UserId == null
                   ? Enumerable.Empty<string>()
                   : await _cache.GetRightsOfUserInEvent(_userId.UserId.Value, query.EventId);
    }
}