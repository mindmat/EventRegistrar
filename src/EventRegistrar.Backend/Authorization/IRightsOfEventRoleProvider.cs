using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Events.UsersInEvents;

namespace EventRegistrar.Backend.Authorization
{
    internal interface IRightsOfEventRoleProvider
    {
        IEnumerable<string> GetRightsOfEventRoles(Guid eventId, ICollection<UserInEventRole> usersRolesInEvent);
    }
}