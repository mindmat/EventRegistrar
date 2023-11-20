using EventRegistrar.Backend.Events.UsersInEvents;

using Microsoft.Extensions.Caching.Memory;

namespace EventRegistrar.Backend.Authorization;

public class RightsOfUserInEventCache(IQueryable<UserInEvent> usersInEventsInEvents,
                                      IMemoryCache memoryCache,
                                      IRightsOfEventRoleProvider rightsOfEventRoleProvider)
{
    private readonly TimeSpan _slidingExpiration = new(0, 1, 0, 0);


    public async Task<HashSet<string>> GetRightsOfUserInEvent(Guid userId, Guid eventId)
    {
        var key = new UserInEventCacheKey(userId, eventId);
        return await memoryCache.GetOrCreateAsync(key, entry => CreateRightsOfUserInEventCacheEntry(entry, userId, eventId));
    }

    private async Task<HashSet<string>> CreateRightsOfUserInEventCacheEntry(ICacheEntry entry, Guid userId, Guid eventId)
    {
        entry.SlidingExpiration = _slidingExpiration;
        var usersRolesInEvent = await usersInEventsInEvents.Where(uie => uie.UserId == userId
                                                                      && uie.EventId == eventId)
                                                           .Select(uie => uie.Role)
                                                           .ToListAsync();

        return rightsOfEventRoleProvider.GetRightsOfEventRoles(eventId, usersRolesInEvent)
                                        .OrderBy(rgt => rgt)
                                        .ToHashSet();
    }
}

public class UserInEventCacheKey(Guid userId, Guid eventId)
{
    private readonly Guid _eventId = eventId;
    private readonly Guid _userId = userId;

    public override bool Equals(object? obj)
    {
        return obj is UserInEventCacheKey key && _userId.Equals(key._userId) && _eventId.Equals(key._eventId);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_userId, _eventId);
    }
}