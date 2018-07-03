using EventRegistrar.Backend.Authentication;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public class AuthenticatedUser
    {
        public AuthenticatedUser(IdentityProvider identityProvider,
                                 string identityProviderUserIdentifier,
                                 string firstName,
                                 string lastName,
                                 string email)
        {
            IdentityProvider = identityProvider;
            IdentityProviderUserIdentifier = identityProviderUserIdentifier;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }

        public string Email { get; }
        public string FirstName { get; }
        public IdentityProvider IdentityProvider { get; }
        public string IdentityProviderUserIdentifier { get; }
        public string LastName { get; }
    }
}