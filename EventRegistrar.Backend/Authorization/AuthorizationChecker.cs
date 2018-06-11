using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EventRegistrar.Backend.Authorization
{
    internal class AuthorizationChecker : IAuthorizationChecker
    {
        private readonly AuthenticatedUser _user;
        private readonly IQueryable<User> _users;

        public AuthorizationChecker(AuthenticatedUser user, IQueryable<User> users)
        {
            _user = user;
            _users = users;
        }

        public async Task ThrowIfUserIsNotInEventRole(Guid eventId, UserInEventRole role)
        {
            var ok = await _users.AnyAsync(usr => usr.Id == _user.UserId &&
                                                  usr.Events.Any(uie => uie.EventId == eventId &&
                                                                        uie.Role >= role));
            if (!ok)
            {
                throw new UnauthorizedAccessException($"You are not authorized for role {role} in event {eventId}");
            }
        }
    }
}