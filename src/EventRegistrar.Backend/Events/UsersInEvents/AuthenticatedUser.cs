using EventRegistrar.Backend.Authentication;

namespace EventRegistrar.Backend.Events.UsersInEvents;

public class AuthenticatedUser
{
    private AuthenticatedUser() { }

    public AuthenticatedUser(IdentityProvider identityProvider,
                             string identityProviderUserIdentifier,
                             string? firstName = null,
                             string? lastName = null,
                             string? email = null,
                             string? avatarUrl = null)
    {
        IdentityProvider = identityProvider;
        IdentityProviderUserIdentifier = identityProviderUserIdentifier;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        AvatarUrl = avatarUrl;
    }

    public IdentityProvider IdentityProvider { get; }
    public string IdentityProviderUserIdentifier { get; }
    public string? FirstName { get; }
    public string? LastName { get; }
    public string? Email { get; }
    public string? AvatarUrl { get; }

    public static AuthenticatedUser None => new();
}