using System;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authentication;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public interface IAuthenticatedUserProvider
    {
        IdentityProvider IdentityProvider { get; }
        string IdentityProviderUserIdentifier { get; }

        Task<Guid?> GetAuthenticatedUserId();
    }
}