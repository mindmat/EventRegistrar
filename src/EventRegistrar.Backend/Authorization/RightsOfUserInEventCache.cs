using EventRegistrar.Backend.Events.UsersInEvents;

using Microsoft.Extensions.Caching.Memory;

namespace EventRegistrar.Backend.Authorization;

public class RightsOfUserInEventCache
{
    private readonly IQueryable<UserInEvent> _usersInEvents;
    private readonly IMemoryCache _memoryCache;
    private readonly IRightsOfEventRoleProvider _rightsOfEventRoleProvider;
    private readonly TimeSpan _slidingExpiration = new(0, 1, 0, 0);

    public RightsOfUserInEventCache(IQueryable<UserInEvent> usersInEventsInEvents,
                                    IMemoryCache memoryCache,
                                    IRightsOfEventRoleProvider rightsOfEventRoleProvider)
    {
        _usersInEvents = usersInEventsInEvents;
        _memoryCache = memoryCache;
        _rightsOfEventRoleProvider = rightsOfEventRoleProvider;
    }


    public async Task<HashSet<string>> GetRightsOfUserInEvent(Guid userId, Guid eventId)
    {
        var key = new UserInEventCacheKey(userId, eventId);
        return await _memoryCache.GetOrCreateAsync(key, entry => CreateRightsOfUserInEventCacheEntry(entry, userId, eventId));
    }

    private async Task<HashSet<string>> CreateRightsOfUserInEventCacheEntry(ICacheEntry entry, Guid userId, Guid eventId)
    {
        entry.SlidingExpiration = _slidingExpiration;
        var usersRolesInEvent = await _usersInEvents.Where(uie => uie.UserId == userId && uie.EventId == eventId)
                                                    .Select(uie => uie.Role)
                                                    .ToListAsync();

        return _rightsOfEventRoleProvider.GetRightsOfEventRoles(eventId, usersRolesInEvent)
                                         .ToHashSet();
    }
}

public class UserInEventCacheKey
{
    private readonly Guid _eventId;
    private readonly Guid _userId;

    public UserInEventCacheKey(Guid userId, Guid eventId)
    {
        _userId = userId;
        _eventId = eventId;
    }

    public override bool Equals(object? obj)
    {
        return obj is UserInEventCacheKey key && _userId.Equals(key._userId) && _eventId.Equals(key._eventId);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_userId, _eventId);
    }
}