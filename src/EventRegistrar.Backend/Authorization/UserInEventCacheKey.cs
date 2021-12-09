namespace EventRegistrar.Backend.Authorization;

public class UserInEventCacheKey
{
    private readonly Guid _eventId;
    private readonly Guid _userId;

    public UserInEventCacheKey(Guid userId, Guid eventId)
    {
        _userId = userId;
        _eventId = eventId;
    }

    public override bool Equals(object obj)
    {
        return obj is UserInEventCacheKey key &&
               _userId.Equals(key._userId) &&
               _eventId.Equals(key._eventId);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_userId, _eventId);
    }
}