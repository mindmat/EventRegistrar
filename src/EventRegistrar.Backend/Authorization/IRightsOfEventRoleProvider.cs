using EventRegistrar.Backend.Events.UsersInEvents;

namespace EventRegistrar.Backend.Authorization;

public interface IRightsOfEventRoleProvider
{
    IEnumerable<string> GetRightsOfEventRoles(Guid eventId, ICollection<UserInEventRole> usersRolesInEvent);
}