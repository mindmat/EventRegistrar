using EventRegistrator.Functions.Events;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EventRegistrator.Functions.Users
{
    public static class UsersExtensionMethods
    {
        public static async Task AssertUserIsInEventRole(this IQueryable<User> users, Guid userId, Guid eventId, UserInEventRole requestedRole)
        {
            var ok = await users.AnyAsync(usr => usr.Id == userId &&
                                                 usr.Events.Any(uie => uie.EventId == eventId &&
                                                                       uie.Role >= requestedRole));
            if (!ok)
            {
                throw new UnauthorizedAccessException($"You are not authorized to administrate event {eventId}");
            }
        }

        public static async Task AssertUserIsInEventRole(this IQueryable<User> users, Guid eventId, UserInEventRole requestedRole, TraceWriter log)
        {
            var user = await users.GetAuthenticatedUser(log);
            var ok = await users.AnyAsync(usr => usr.Id == user.Id &&
                                                 usr.Events.Any(uie => uie.EventId == eventId &&
                                                                       uie.Role >= requestedRole));
            if (!ok)
            {
                throw new UnauthorizedAccessException($"You are not authorized to administrate event {eventId}");
            }
        }

        public static async Task<User> GetAuthenticatedUser(this IQueryable<User> users, TraceWriter log)
        {
            if (ClaimsPrincipal.Current?.Identity?.IsAuthenticated == true)
            {
                var provider = ClaimsPrincipal.Current.Identity.AuthenticationType;
                var identifier = ClaimsPrincipal.Current.Identity.AuthenticationType;
                log.Info($"Provider: {provider}, Identifier: {identifier}, claims: {ClaimsPrincipal.Current.Claims.ToString()}");
                var user = await users.FirstOrDefaultAsync(usr => usr.IdentityProvider == provider && usr.IdentityProviderUserIdentifier == identifier);
                return user;
            }
            throw new UnauthorizedAccessException("No user is authenticated, please log in");
        }
    }
}