using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;

namespace EventRegistrar.Backend.Authorization;

internal class AuthorizationChecker : IAuthorizationChecker
{
    private readonly SourceQueueProvider _sourceQueueProvider;
    private readonly RightsOfUserInEventCache _cache;
    private readonly AuthenticatedUserId _user;

    public AuthorizationChecker(AuthenticatedUserId user,
                                SourceQueueProvider sourceQueueProvider,
                                RightsOfUserInEventCache cache)
    {
        _user = user;
        _sourceQueueProvider = sourceQueueProvider;
        _cache = cache;
    }

    public async Task ThrowIfUserHasNotRight(Guid eventId, string requestTypeName)
    {
        if (_sourceQueueProvider.SourceQueueName != null)
            // message from a queue, no user is authenticated
        {
            return;
        }

        if (!_user.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("You are not authenticated");
        }

        var rightsOfUserInEvent = await _cache.GetRightsOfUserInEvent(_user.UserId.Value, eventId);
        if (!rightsOfUserInEvent.Contains(requestTypeName))
        {
            throw new UnauthorizedAccessException($"You ({_user.UserId}) are not authorized for {requestTypeName} in event {eventId}");
        }
    }

    public async Task<bool> UserHasRight(Guid eventId, string requestTypeName)
    {
        if (_sourceQueueProvider.SourceQueueName != null)
            // message from a queue, no user is authenticated
        {
            return true;
        }

        if (!_user.UserId.HasValue)
        {
            return false;
        }

        var rightsOfUserInEvent = await _cache.GetRightsOfUserInEvent(_user.UserId.Value, eventId);
        return rightsOfUserInEvent.Contains(requestTypeName);
    }
}