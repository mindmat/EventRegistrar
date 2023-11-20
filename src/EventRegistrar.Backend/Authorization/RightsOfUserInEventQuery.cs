using EventRegistrar.Backend.Events.UsersInEvents;

namespace EventRegistrar.Backend.Authorization;

public class RightsOfUserInEventQuery : IRequest<IEnumerable<string>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class RightsOfUserInEventQueryHandler(AuthenticatedUserId userId,
                                             RightsOfUserInEventCache cache)
    : IRequestHandler<RightsOfUserInEventQuery, IEnumerable<string>>
{
    public async Task<IEnumerable<string>> Handle(RightsOfUserInEventQuery query, CancellationToken cancellationToken)
    {
        return userId.UserId == null
                   ? Enumerable.Empty<string>()
                   : await cache.GetRightsOfUserInEvent(userId.UserId.Value, query.EventId);
    }
}