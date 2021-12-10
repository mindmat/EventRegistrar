using EventRegistrar.Backend.Events.UsersInEvents;

namespace EventRegistrar.Backend.Authentication;

public interface IIdentityProvider
{
    (IdentityProvider? Provider, string? Identifier) GetIdentifier(IHttpContextAccessor contextAccessor);

    AuthenticatedUser GetUser(IHttpContextAccessor httpContextAccessor);
}