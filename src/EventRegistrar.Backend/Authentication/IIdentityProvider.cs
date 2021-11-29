using EventRegistrar.Backend.Events.UsersInEvents;
using Microsoft.AspNetCore.Http;

namespace EventRegistrar.Backend.Authentication
{
    public interface IIdentityProvider
    {
        IdentityProvider Provider { get; }

        string? GetIdentifier(IHttpContextAccessor contextAccessor);

        AuthenticatedUser GetUser(IHttpContextAccessor httpContextAccessor);
    }
}