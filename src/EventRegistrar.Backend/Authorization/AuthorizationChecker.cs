using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;

namespace EventRegistrar.Backend.Authorization;

internal class AuthorizationChecker(AuthenticatedUserId user,
                                    SourceQueueProvider sourceQueueProvider,
                                    RightsOfUserInEventCache cache)
    : IAuthorizationChecker
{
    public async Task ThrowIfUserHasNotRight(Guid eventId, string requestTypeName)
    {
        if (sourceQueueProvider.SourceQueueName != null)
            // message from a queue, no user is authenticated
        {
            return;
        }

        if (!user.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("You are not authenticated");
        }

        var rightsOfUserInEvent = await cache.GetRightsOfUserInEvent(user.UserId.Value, eventId);
        if (!rightsOfUserInEvent.Contains(requestTypeName))
        {
            throw new UnauthorizedAccessException($"You ({user.UserId}) are not authorized for {requestTypeName} in event {eventId}");
        }
    }

    public async Task<bool> UserHasRight(Guid eventId, string requestTypeName)
    {
        if (sourceQueueProvider.SourceQueueName != null)
            // message from a queue, no user is authenticated
        {
            return true;
        }

        if (!user.UserId.HasValue)
        {
            return false;
        }

        var rightsOfUserInEvent = await cache.GetRightsOfUserInEvent(user.UserId.Value, eventId);
        return rightsOfUserInEvent.Contains(requestTypeName);
    }
}